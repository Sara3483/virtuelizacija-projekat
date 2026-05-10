using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using BaterijaContract;
using System.ServiceModel;
using System.IO;

namespace VirtuelizacijaProjekat
{
    public class EisService : IEisService
    {
        private static int previousIndex = -1;
        private static EisFileWriter fileWriter;
        private static string rejectedPath;
        public BaterijaResponse StartSession(EisMeta meta)
        {
            if(meta == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault
                    {
                        Message = "Session metadata not provided.",
                        RowIndex = -1
                    });
            }

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
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "T_degC value must be a real number.",
                        Field = "T_degC"
                    });
            }

            if(fileWriter == null)
            {
                WriteReject("Session has not started.");
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Session has not been started.",
                        Field = "Session"
                    });
            }

            fileWriter.WriteSample(sample);

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

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session completed!",
                Status = BaterijaStatus.COMPLETED
            };
        }
    }
}
