using BaterijaContract.Events;
using System;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsoleTransferSubscriber
    {
        public void TransferStartedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine(e.Message);
            Console.WriteLine("========================================");
        }

        public void SampleReceivedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("[SAMPLE] RowIndex: " + e.RowIndex);
        }

        public void TransferCompletedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(e.Message);
            Console.WriteLine("========================================");
            Console.WriteLine();
        }

        public void WarningRaisedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine(e.Message);
            Console.WriteLine("RowIndex: " + e.RowIndex);
            Console.WriteLine("----------------------------------------");
        }
    }
}