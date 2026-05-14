using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Common
{
    [ServiceContract]
    public interface IEisService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        BaterijaResponse StartSession(EisMeta meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        BaterijaResponse PushSample(EisSample sample);

        [OperationContract]
        BaterijaResponse EndSession();
    }
}
