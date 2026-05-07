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
        private int previousIndex = -1;

        private EisFileWriter fileWriter;
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

            string folderPath = "EisOutput";
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, meta.FileName);
            fileWriter = new EisFileWriter(filePath);

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session started!",
                Status = BaterijaStatus.IN_PROGRESS
            };
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
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Row index must be greater than previous.",
                        Field = "RowIndex"
                    });
            }

            if(sample.FrequencyHz <= 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Frequency must be greater than 0.",
                        Field = "FrequencyHz"
                    });
            }

            if(double.IsNaN(sample.R_ohm) || double.IsInfinity(sample.R_ohm))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "R_ohm value must be a real number.",
                        Field = "R_ohm"
                    });
            }

            if(double.IsNaN(sample.T_degC) || double.IsInfinity(sample.T_degC))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "T_degC value must be a real number.",
                        Field = "T_degC"
                    });
            }

            if(fileWriter == null)
            {
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

            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session completed!",
                Status = BaterijaStatus.COMPLETED
            };
        }
    }
}
