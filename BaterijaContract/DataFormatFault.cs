using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace BaterijaContract
{
    [DataContract]
    public class DataFormatFault
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public int RowIndex { get; set; }
    }
}
