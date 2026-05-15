using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace VirtuelizacijaProjekat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] //koristi jedan servis za sve pozive

    public class EisService : IEisService
    {
        private static int previousIndex = -1;
        private static EisFileWriter fileWriter;
        private static string rejectedPath;

        public event EventHandler<TransferEventArgs> OnTransferStarted;
        public event EventHandler<TransferEventArgs> OnSampleReceived;
        public event EventHandler<TransferEventArgs> OnTransferCompleted;
        public event EventHandler<TransferEventArgs> OnWarningRaised;

        public event EventHandler<TransferEventArgs> OnPhaseAngleShift;
        private double? previousPhi = null;
        private EisMeta currentMeta;

        public event EventHandler<TransferEventArgs> OnRatioOutOfBounds;
        public event EventHandler<TransferEventArgs> OnRatioWarning;
        private double qSum = 0;
        private int qCount = 0;

        public BaterijaResponse StartSession(EisMeta meta)
        {
            previousPhi = null;
            Console.WriteLine("Data transferring in progress...");

            qSum = 0;
            qCount = 0;

            //okidanje eventa
            RaiseTransferStarted("Transfer started.");

            if (meta == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = "Session metadata not provided.",
                        RowIndex = -1
                    });
            }

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
            Console.WriteLine("REJECT PATH: " + Path.GetFullPath(rejectedPath));
            if (!string.IsNullOrEmpty(rejectedPath))
            {
                File.AppendAllText(rejectedPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{reason}{Environment.NewLine}");
            }
        }

        public BaterijaResponse PushSample(EisSample sample)
        {
            if(sample == null)
            {
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
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
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
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
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
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
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
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
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "T_degC value must be a real number.",
                        Field = "T_degC"
                    });
            }

            //analitika 1
            double phi = Math.Atan2(sample.X_ohm, sample.R_ohm);

            if (previousPhi.HasValue)
            {
                double deltaPhi = phi - previousPhi.Value;

                double phiThreshold = double.Parse(
                    ConfigurationManager.AppSettings["PhiThreshold"],
                    System.Globalization.CultureInfo.InvariantCulture);

                if (Math.Abs(deltaPhi) > phiThreshold)
                {
                    string direction;

                    if (deltaPhi > 0)
                    {
                        direction = "Shift toward inductive behavior";
                    }
                    else
                    {
                        direction = "Shift toward capacitive behavior";
                    }

                    RaisePhaseAngleShift(
                        $"Phase angle shift detected: {direction}",
                        phi,
                        deltaPhi,
                        sample.FrequencyHz,
                        currentMeta.SoCPercentage,
                        direction);
                }
            }

            previousPhi = phi;

            //analitika 2
            double q = Math.Abs(sample.X_ohm) / sample.R_ohm;
            double qMin = double.Parse(ConfigurationManager.AppSettings["QMin"],
                System.Globalization.CultureInfo.InvariantCulture);
            double qMax = double.Parse(ConfigurationManager.AppSettings["QMax"],
                System.Globalization.CultureInfo.InvariantCulture);
            double qDev = double.Parse(ConfigurationManager.AppSettings["QDev"],
                System.Globalization.CultureInfo.InvariantCulture);
            qSum += q;
            qCount++;
            double qMean = qSum / qCount;
            if(q < qMin || q > qMax)
            {
                WriteReject($"Reactive ratio out of bounds {q}");
                RaiseRatioOutOfBounds("Reactive ratio out of bounds.",
                    sample.RowIndex, q, qMean, sample.FrequencyHz);
            }

            if(q < (1 - qDev) * qMean)
            {
                RaiseRatioWarning("Reactive ratio below expected.",
                    sample.RowIndex, q, qMean, sample.FrequencyHz, "below expected");
            }
            else if(q > (1 - qDev) * qMean)
            {
                RaiseRatioWarning("Reactive ratio above expected.",
                    sample.RowIndex, q, qMean, sample.FrequencyHz, "above expected");
            }

            if (fileWriter == null)
            {
                WriteReject("Session has not started.");
                RaiseWarning("Warning: invalid sample", sample.RowIndex);
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Session has not been started.",
                        Field = "Session"
                    });
            }

            fileWriter.WriteSample(sample);

            Console.WriteLine($"Data transfer in progress...");
            RaiseSampleReceived("Sample received", sample.RowIndex);

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

            Console.WriteLine("Transfer completed.");
            RaiseTransferCompleted("Transfer successfully completed.");

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session completed!",
                Status = BaterijaStatus.COMPLETED
            };
        }

        //metode za okidanje dogadjaja
        private void RaiseTransferStarted(string message)
        {
            OnTransferStarted?.Invoke(this, new TransferEventArgs(message));
        }

        private void RaiseSampleReceived(string message, int rowIndex)
        {
            OnSampleReceived?.Invoke(this, new TransferEventArgs(message, rowIndex));
        }

        private void RaiseTransferCompleted(string message)
        {
            OnTransferCompleted?.Invoke(this, new TransferEventArgs(message));
        }

        private void RaiseWarning(string message, int rowIndex = -1)
        {
            OnWarningRaised?.Invoke(this, new TransferEventArgs(message, rowIndex));
        }

        private void RaisePhaseAngleShift(string message, double phi, double deltaPhi, double frequency, double soc, string direction)
        {
            OnPhaseAngleShift?.Invoke(this, new TransferEventArgs(message)
            { Phi = phi, DeltaPhi = deltaPhi, FrequencyHz = frequency, SoC = soc, ShiftDirection = direction });
        }

        private void RaiseRatioOutOfBounds(string message, int rowIndex, double q, double qMean, double frequency)
        {
            OnRatioOutOfBounds?.Invoke(this, new TransferEventArgs(message, rowIndex)
            { Q = q, QMean = qMean, FrequencyHz = frequency, SoC = currentMeta.SoCPercentage, BatteryId = currentMeta.BatteryId });
        }

        private void RaiseRatioWarning(string message, int rowIndex, double q, double qMean, double frequency, string direction)
        {
            OnRatioWarning?.Invoke(this, new TransferEventArgs(message, rowIndex)
            { Q = q, QMean = qMean, FrequencyHz = frequency, SoC = currentMeta.SoCPercentage, BatteryId = currentMeta.BatteryId, ShiftDirection = direction });
        }
    }
}
