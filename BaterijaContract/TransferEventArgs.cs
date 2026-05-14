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

        public TransferEventArgs(string message, int rowIndex = -1)
        {
            Message = message;
            RowIndex = rowIndex;
        }
    }
}
