using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaterijaContract;

namespace VirtuelizacijaProjekat
{
    public class EisFileWriter : IDisposable
    {
        private FileStream fileStream;
        private StreamWriter writer;
        private bool disposed = false;

        public EisFileWriter(string fPath)
        { 
            fileStream = new FileStream(fPath, FileMode.Create, FileAccess.Write);
            writer = new StreamWriter(fileStream);

            writer.WriteLine("RowIndex,FrequencyHz,R_ohm,T_degC,Range_ohm,TimestampLocal");
        }

        public void WriteSample(EisSample sample)
        {
            if(disposed)
            {
                throw new ObjectDisposedException(nameof(EisFileWriter));
            }

            writer.WriteLine($"{sample.RowIndex}, {sample.FrequencyHz}, {sample.R_ohm}," +
                $"{sample.T_degC}, {sample.Range_ohm}, {sample.TimestampLocal}");

            writer.Flush(); //za oporavak u slucaju prekida - odmah upisuje na disk info
        }

        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool disposing)
        {
            if(!disposed)
            {
                if (disposing)
                {
                    writer?.Dispose();
                    fileStream?.Dispose();
                }
                disposed = true;
            }
        }
    }
}
