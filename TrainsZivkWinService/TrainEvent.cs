using System;

namespace TrainsZivkWinService
{
    public class TrainEvent
    {

        public string DObjName { get; set; }

        public short DObjType { get; set; }

        public string EventAxis { get; set; }

        public string EventStation { get; set; }

        public DateTime EventTime { get; set; }

        public short EventType { get; set; }

        public Guid TrainId { get; set; }

        public string TrainNumber { get; set; }

        public string TrainNumberPrefix { get; set; }

        public string TrainNumberSuffix { get; set; }

        public string OperationCode { get; set; }

        public DateTime MessTime { get; set; }

        public string Index1 { get; set; }

        public string Index2 { get; set; }

        public string Index3 { get; set; }

    }
}
