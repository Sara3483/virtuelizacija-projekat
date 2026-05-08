using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using BaterijaContract;

namespace VirtuelizacijaProjekat
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EisService));
            try
            {
                host.Open();
                Console.WriteLine("WCF service running...");
                Console.ReadLine();
                host.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                host.Abort();
            }

            string rootPath = @"C:\Users\Sara\Desktop\Baza\SoC Estimation on Li-ion Batteries A New EIS-based Dataset for data-driven applications";
            EisFileReader reader = new EisFileReader(rootPath);
            EisService service = new EisService();

            List<FileInfo> files = reader.GetCsv();
            Console.WriteLine("Found CSV files: " + files.Count);
            foreach(FileInfo file in files)
            {
                Console.WriteLine("Processing file: " + file.Name);
                List<EisSample> samples = reader.ReadSamples(file);
                EisMeta meta = reader.CreateMeta(file, samples.Count);
                service.StartSession(meta);
                foreach(EisSample sample in samples)
                {
                    service.PushSample(sample);
                }
                service.EndSession();
                Console.WriteLine("Finished: " + file.Name);
            }
            Console.WriteLine("Done.");
            Console.Read();
        }
    }
}
