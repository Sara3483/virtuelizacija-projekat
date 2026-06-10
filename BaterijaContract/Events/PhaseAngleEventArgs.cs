using System;

namespace BaterijaContract.Events
{
    public class PhaseAngleEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int RowIndex { get; set; }
        public string BatteryId { get; set; }
        public double SoC { get; set; }
        public double FrequencyHz { get; set; }
        public double Phi { get; set; }
        public double DeltaPhi { get; set; }
        public string ShiftDirection { get; set; }

        public PhaseAngleEventArgs(string mess, int ri, string bid, double soc, double freq, double p, double dp, string shift)
        {
            Message = mess;
            RowIndex = ri;
            BatteryId = bid;
            SoC = soc;
            FrequencyHz = freq;
            Phi = p;
            DeltaPhi = dp;
            ShiftDirection = shift;
        }
    }
}
