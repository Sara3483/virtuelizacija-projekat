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
        }
    }
}
