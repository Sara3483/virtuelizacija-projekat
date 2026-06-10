using System;

namespace BaterijaContract.Events
{
    public class ReactiveRatioEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int RowIndex { get; set; }
        public string BatteryId { get; set; }
        public double SoC { get; set; }
        public double FrequencyHz { get; set; }
        public double Q { get; set; }
        public double QMean { get; set; }
        public string Direction { get; set; }

        public ReactiveRatioEventArgs(
            string message,
            int rowIndex,
            string batteryId,
            double soc,
            double frequencyHz,
            double q,
            double qMean,
            string direction)
        {
            Message = message;
            RowIndex = rowIndex;
            BatteryId = batteryId;
            SoC = soc;
            FrequencyHz = frequencyHz;
            Q = q;
            QMean = qMean;
            Direction = direction;
        }
    }
}