using System;
using System.IO;
using BaterijaContract.Events;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class FilePhaseAngleSubscriber
    {
        private readonly string logPath;

        public FilePhaseAngleSubscriber(string log)
        {
            this.logPath = log;
        }

        public void PhaseAngleShiftHandler(object sender, PhaseAngleEventArgs e)
        {
            string text = "[PHASE ANGLE SHIFT] " + e.Message +
                " | Direction: " + e.ShiftDirection +
                " | RowIndex: " + e.RowIndex +
                " | BatteryId: " + e.BatteryId +
                " | SoC: " + e.SoC +
                " | FrequencyHz: " + e.FrequencyHz +
                " | Phi: " + e.Phi +
                " | DeltaPhi: " + e.DeltaPhi;
            WriteLog(text);
        }

        private void WriteLog(string text)
        {
            File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + text + Environment.NewLine);
        }
    }
}
