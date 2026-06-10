using BaterijaContract.Events;
using System;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsoleReactiveRatioSubscriber
    {
        public void ReactiveRatioOutOfBoundsHandler(object sender, ReactiveRatioEventArgs e)
        {
            Console.WriteLine("[REACTIVE RATIO OUT OF BOUNDS] " + e.Message +
                " | Direction: " + e.Direction +
                " | RowIndex: " + e.RowIndex +
                " | BatteryId: " + e.BatteryId +
                " | SoC: " + e.SoC +
                " | FrequencyHz: " + e.FrequencyHz +
                " | Q: " + e.Q +
                " | QMean: " + e.QMean);
        }

        public void ReactiveRatioWarningHandler(object sender, ReactiveRatioEventArgs e)
        {
            Console.WriteLine("[REACTIVE RATIO WARNING] " + e.Message +
                " | Direction: " + e.Direction +
                " | RowIndex: " + e.RowIndex +
                " | BatteryId: " + e.BatteryId +
                " | SoC: " + e.SoC +
                " | FrequencyHz: " + e.FrequencyHz +
                " | Q: " + e.Q +
                " | QMean: " + e.QMean);
        }
    }
}