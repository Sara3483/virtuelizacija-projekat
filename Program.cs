using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using Common;

namespace VirtuelizacijaProjekat
{
    class Program
    {
        static void Main(string[] args)
        {
            EisService service = new EisService();
            service.OnTransferStarted += (sender, e) =>
            {
                Console.WriteLine(e.Message);
            };

            service.OnSampleReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Message}. RowIndex: {e.RowIndex}");
            };

            service.OnTransferCompleted += (sender, e) =>
            {
                Console.WriteLine(e.Message);
            };

            service.OnWarningRaised += (sender, e) =>
            {
                Console.WriteLine($"{e.Message}. RowIndex: {e.RowIndex}");
            };

            service.OnPhaseAngleShift += (sender, e) =>
            {
                Console.WriteLine($"{e.Message} | Phi: {e.Phi} | DeltaPhi: {e.DeltaPhi}" +
                    $"| FrequencyHz: {e.FrequencyHz} | SoC: {e.SoC}");
            };

            ServiceHost host = new ServiceHost(service);

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
        }
    }
}
