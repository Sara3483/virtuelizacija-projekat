using BaterijaContract.Events;
using System;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsoleTransferSubscriber
    {
        public void TransferStartedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("[START] " + e.Message);
        }

        public void SampleReceivedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("[SAMPLE] " + e.Message + "RowIndex: " + e.RowIndex);
        }

        public void TransferCompletedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("[COMPLETED] " + e.Message);
        }

        public void WarningRaisedHandler(object sender, CustomEventArgs e)
        {
            Console.WriteLine("[WARNING] " + e.Message + "RowIndex: " + e.RowIndex);
        }

    }
}
