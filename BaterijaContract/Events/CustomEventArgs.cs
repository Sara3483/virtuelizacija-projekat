using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaterijaContract.Events
{
    public class CustomEventArgs
    {
        public string Message { get; set; }
        public int RowIndex { get; set; }
        public string BatteryId { get; set; }
        public string TestId { get; set; }
        public string SoC { get; set; }

        public CustomEventArgs(string message)
        {
            Message = message;
            RowIndex = -1;
        }

        public CustomEventArgs(string message, int rowIndex)
        {
            Message = message;
            RowIndex = rowIndex;
        }

        public CustomEventArgs(string message, int rowIndex, string batteryId, string testId, string soc)
        {
            Message = message;
            RowIndex = rowIndex;
            BatteryId = batteryId;
            TestId = testId;
            SoC = soc;
        }
    }
}
