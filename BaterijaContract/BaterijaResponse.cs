using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace BaterijaContract
{
    [DataContract]
    public class BaterijaResponse
    {
        [DataMember]
        public bool ACK { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public BaterijaStatus Status { get; set; }
    }
}
