using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using BaterijaContract;

namespace VirtuelizacijaProjekat
{
    public class EisService : IEisService
    {
        public BaterijaResponse StartSession(EisMeta meta)
        {
            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session started!",
                Status = BaterijaStatus.IN_PROGRESS
            };
        }

        public BaterijaResponse PushSample(EisSample sample)
        {
            return new BaterijaResponse
            {
                ACK = true,
                Message = "Sample recieved!",
                Status = BaterijaStatus.IN_PROGRESS
            };
        }

        public BaterijaResponse EndSession()
        {
            return new BaterijaResponse
            {
                ACK = true,
                Message = "Session completed!",
                Status = BaterijaStatus.COMPLETED
            };
        }
    }
}
