using Common;
using System;
using System.IO;
using System.ServiceModel;
using System.Configuration;
using VirtuelizacijaProjekat.Events;
using VirtuelizacijaProjekat.Analytics;

namespace VirtuelizacijaProjekat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] //koristi jedan servis za sve pozive

    public class EisService : IEisService
    {
        private static int previousIndex = -1;
        private static EisFileWriter fileWriter;
        private static string rejectedPath;

        private readonly TransferEventPublisher transferEventPublisher;
        private readonly PhaseAngleProcessor phaseAngleProcessor;
        private readonly ReactiveRatioProcessor reactiveRatioProcessor;

        public EisService(TransferEventPublisher transfer, PhaseAngleProcessor phase, ReactiveRatioProcessor reactive)
        {
            this.transferEventPublisher = transfer;
            this.phaseAngleProcessor = phase;
            this.reactiveRatioProcessor = reactive;
        }

        private EisMeta currentMeta;

        public BaterijaResponse StartSession(EisMeta meta)
        {
            if (meta == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = "Session metadata not provided.",
                        RowIndex = -1
                    });
            }

            phaseAngleProcessor.Reset();
            reactiveRatioProcessor.Reset();

            //okidanje eventa
            string transferStartedMessage = ConfigurationManager.AppSettings["TransferStartedMessage"];
            transferEventPublisher.RaiseTransferStarted(transferStartedMessage);

            currentMeta = meta;

            previousIndex = -1;

            string folderPath = Path.Combine("Data", meta.BatteryId,
                meta.TestId, meta.SoCPercentage.ToString());

            Directory.CreateDirectory(folderPath);
            string sessionPath = Path.Combine(folderPath, "session.csv");

            rejectedPath = Path.Combine(folderPath, "rejects.csv");
            if (!File.Exists(rejectedPath))
            {
                File.WriteAllText(rejectedPath, "Time,Reason\n");
            }

            fileWriter?.Dispose();

            fileWriter = new EisFileWriter(sessionPath);

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session started!",
                Status = BaterijaStatus.IN_PROGRESS
            };  
        }

        //funkcija za rejected uzorke
        private void WriteReject(string reason)
        {
            if (!string.IsNullOrEmpty(rejectedPath))
            {
                File.AppendAllText(rejectedPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{reason}{Environment.NewLine}");
            }
        }

        public BaterijaResponse PushSample(EisSample sample)
        {
            string warningMessage = ConfigurationManager.AppSettings["WarningMessage"];
            
            if(sample == null)
            {
                transferEventPublisher.RaiseWarning(warningMessage, -1);
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = "Sample not forwarded.",
                        RowIndex = -1
                    });
            }

            if(sample.RowIndex <= previousIndex)
            {
                WriteReject("Row index must be greater than previous.");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Row index must be greater than previous.",
                        Field = "RowIndex"
                    });
            }

            if(sample.FrequencyHz <= 0)
            {
                WriteReject("Frequency must be greater than 0.");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Frequency must be greater than 0.",
                        Field = "FrequencyHz"
                    });
            }

            if(double.IsNaN(sample.R_ohm) || double.IsInfinity(sample.R_ohm))
            {
                WriteReject("R_ohm value must be a real number");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "R_ohm value must be a real number.",
                        Field = "R_ohm"
                    });
            }

            if(double.IsNaN(sample.T_degC) || double.IsInfinity(sample.T_degC))
            {
                WriteReject("T_degC must be a real number.");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "T_degC value must be a real number.",
                        Field = "T_degC"
                    });
            }

            if (fileWriter == null)
            {
                WriteReject("Session has not started.");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Session has not been started.",
                        Field = "Session"
                    });
            }

            phaseAngleProcessor.ProcessSample(sample, currentMeta);

            try
            {
                reactiveRatioProcessor.ProcessSample(sample, currentMeta, WriteReject);
            }
            catch(InvalidOperationException)
            {
                WriteReject("R_ohm value is zero!");
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "R_ohm value is zero!",
                        Field = "R_ohm"
                    });
            }

            fileWriter.WriteSample(sample);

            string sampleReceivedMessage = ConfigurationManager.AppSettings["SampleReceivedMessage"];
            transferEventPublisher.RaiseSampleReceived(sampleReceivedMessage, sample.RowIndex);

            previousIndex = sample.RowIndex;

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Sample received!",
                Status = BaterijaStatus.IN_PROGRESS
            };
        }

        public BaterijaResponse EndSession()
        {
            fileWriter?.Dispose();
            fileWriter = null;
            rejectedPath = null;

            string transferCompletedMessage = ConfigurationManager.AppSettings["TransferCompletedMessage"];
            transferEventPublisher.RaiseTransferCompleted(transferCompletedMessage);

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session completed!",
                Status = BaterijaStatus.COMPLETED
            };
        }
    }
}
