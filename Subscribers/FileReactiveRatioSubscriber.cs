using BaterijaContract.Events;
using System;
using System.IO;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class FileReactiveRatioSubscriber
    {
        private readonly string logPath;

        public FileReactiveRatioSubscriber(string logPath)
        {
            this.logPath = logPath;
        }

        public void ReactiveRatioOutOfBoundsHandler(object sender, ReactiveRatioEventArgs e)
        {
            string text = "[REACTIVE RATIO OUT OF BOUNDS] " + e.Message +
                " | Direction: " + e.Direction +
                " | RowIndex: " + e.RowIndex +
                " | BatteryId: " + e.BatteryId +
                " | SoC: " + e.SoC +
                " | FrequencyHz: " + e.FrequencyHz +
                " | Q: " + e.Q +
                " | QMean: " + e.QMean;

            WriteToLog(text);
        }

        public void ReactiveRatioWarningHandler(object sender, ReactiveRatioEventArgs e)
        {
            string text = "[REACTIVE RATIO WARNING] " + e.Message +
                " | Direction: " + e.Direction +
                " | RowIndex: " + e.RowIndex +
                " | BatteryId: " + e.BatteryId +
                " | SoC: " + e.SoC +
                " | FrequencyHz: " + e.FrequencyHz +
                " | Q: " + e.Q +
                " | QMean: " + e.QMean;

            WriteToLog(text);
        }

        private void WriteToLog(string text)
        {
            File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine);
        }
    }
}