using Common;
using System;
using System.IO;
using System.ServiceModel;
using System.Configuration;
using VirtuelizacijaProjekat.Events;
using VirtuelizacijaProjekat.Analytics;

namespace VirtuelizacijaProjekat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EisService : IEisService
    {
        private static int previousIndex = -1;
        private static EisFileWriter fileWriter;
        private static string rejectedPath;

        private readonly TransferEventPublisher transferEventPublisher;
        private readonly PhaseAngleProcessor phaseAngleProcessor;
        private readonly ReactiveRatioProcessor reactiveRatioProcessor;

        private EisMeta currentMeta;

        public EisService(TransferEventPublisher transfer, PhaseAngleProcessor phase, ReactiveRatioProcessor reactive)
        {
            this.transferEventPublisher = transfer;
            this.phaseAngleProcessor = phase;
            this.reactiveRatioProcessor = reactive;
        }

        public BaterijaResponse StartSession(EisMeta meta)
        {
            if (meta == null)
            {
                string reason = "Session metadata not provided.";

                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = reason,
                        RowIndex = -1
                    },
                    reason);
            }

            phaseAngleProcessor.Reset();
            reactiveRatioProcessor.Reset();

            string transferStartedMessage = ConfigurationManager.AppSettings["TransferStartedMessage"];
            transferEventPublisher.RaiseTransferStarted(transferStartedMessage);

            currentMeta = meta;
            previousIndex = -1;

            string folderPath = Path.Combine("Data", meta.BatteryId, meta.TestId, meta.SoCPercentage.ToString());

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

        private void WriteReject(string reason)
        {
            if (!string.IsNullOrEmpty(rejectedPath))
            {
                File.AppendAllText(
                    rejectedPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{reason}{Environment.NewLine}");
            }
        }

        public BaterijaResponse PushSample(EisSample sample)
        {
            string warningMessage = ConfigurationManager.AppSettings["WarningMessage"];

            if (sample == null)
            {
                string reason = "Sample not forwarded.";

                transferEventPublisher.RaiseWarning(warningMessage, -1);

                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = reason,
                        RowIndex = -1
                    },
                    reason);
            }

            if (sample.RowIndex <= previousIndex)
            {
                string reason = "Row index must be greater than previous.";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "RowIndex"
                    },
                    reason);
            }

            if (sample.FrequencyHz <= 0)
            {
                string reason = "Frequency must be greater than 0.";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "FrequencyHz"
                    },
                    reason);
            }

            if (double.IsNaN(sample.R_ohm) || double.IsInfinity(sample.R_ohm))
            {
                string reason = "R_ohm value must be a real number.";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "R_ohm"
                    },
                    reason);
            }

            if (double.IsNaN(sample.T_degC) || double.IsInfinity(sample.T_degC))
            {
                string reason = "T_degC value must be a real number.";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "T_degC"
                    },
                    reason);
            }

            if (fileWriter == null)
            {
                string reason = "Session has not been started.";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "Session"
                    },
                    reason);
            }

            phaseAngleProcessor.ProcessSample(sample, currentMeta);

            try
            {
                reactiveRatioProcessor.ProcessSample(sample, currentMeta, WriteReject);
            }
            catch (InvalidOperationException)
            {
                string reason = "R_ohm value is zero!";

                WriteReject(reason);
                transferEventPublisher.RaiseWarning(warningMessage, sample.RowIndex);

                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = reason,
                        Field = "R_ohm"
                    },
                    reason);
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