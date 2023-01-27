using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace TrainsZivkWinService
{
    [ServiceContract]
    [ServiceKnownType(typeof(TrainEvent))]
    public interface IService
    {

        [OperationContract]
        IList<TrainEvent> GetLastTrainEvents();

    }
}
