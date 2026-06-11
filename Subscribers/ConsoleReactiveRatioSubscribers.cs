using BaterijaContract.Events;
using System;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsoleReactiveRatioSubscriber
    {
        public void ReactiveRatioOutOfBoundsHandler(object sender, ReactiveRatioEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("!!! REACTIVE RATIO OUT OF BOUNDS !!!");
            Console.WriteLine("RowIndex: " + e.RowIndex);
            Console.WriteLine("Q: " + e.Q);
            Console.WriteLine("QMean: " + e.QMean);
            Console.WriteLine("Direction: " + e.Direction);
            Console.WriteLine("-------------------------------------");
        }

        public void ReactiveRatioWarningHandler(object sender, ReactiveRatioEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("!!! REACTIVE RATIO WARNING !!!");
            Console.WriteLine("RowIndex: " + e.RowIndex);
            Console.WriteLine("Q: " + e.Q);
            Console.WriteLine("QMean: " + e.QMean);
            Console.WriteLine("Direction: " + e.Direction);
            Console.WriteLine("--------------------------------");
        }
    }
}