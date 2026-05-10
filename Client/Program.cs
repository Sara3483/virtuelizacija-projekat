using BaterijaContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using VirtuelizacijaProjekat;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ChannelFactory<IEisService> factory = new ChannelFactory<IEisService>("EisServiceEndpoint");

                IEisService service = factory.CreateChannel();

                string rootPath = @"C:\Users\Sara\Desktop\VirtuelizacijaProjekat\Baza\SoC Estimation on Li-ion Batteries A New EIS-based Dataset for data-driven applications";
                EisFileReader reader = new EisFileReader(rootPath);

                List<FileInfo> files = reader.GetCsv();
                Console.WriteLine("Found CSV files: " + files.Count);
                foreach (FileInfo file in files)
                {
                    Console.WriteLine("Processing file: " + file.Name);
                    List<EisSample> samples = reader.ReadSamples(file);
                    EisMeta meta = reader.CreateMeta(file, samples.Count);
                    service.StartSession(meta);
                    foreach (EisSample sample in samples)
                    {
                        try
                        { 
                        service.PushSample(sample);
                        }
                        catch(FaultException<ValidationFault> e)
                        {
                            Console.WriteLine("Validation error: " + e.Detail.Message);
                        }
                        catch(FaultException<DataFormatFault> ex)
                        {
                            Console.WriteLine("Data format error: " + ex.Detail.Message);
                        }
                    }
                    service.EndSession();
                    Console.WriteLine("Finished: " + file.Name);
                }
                ((IClientChannel)service).Close();
                factory.Close();
                Console.WriteLine("Done.");
                Console.ReadLine();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}
