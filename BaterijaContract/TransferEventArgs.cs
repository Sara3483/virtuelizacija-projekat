using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TransferEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int RowIndex { get; set; }

        //podaci za tacku 9
        public double Phi { get; set; }
        public double DeltaPhi { get; set; }
        public double FrequencyHz { get; set; }
        public double SoC { get; set; }
        public string ShiftDirection { get; set; }

        //podaci za tacku 10
        public double Q { get; set; }
        public double QMean { get; set; }
        public string BatteryId { get; set; }

        public TransferEventArgs(string message, int rowIndex = -1)
        {
            Message = message;
            RowIndex = rowIndex;
        }
    }
}
