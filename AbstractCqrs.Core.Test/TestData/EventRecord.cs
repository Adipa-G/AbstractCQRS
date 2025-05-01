using System;

using AbstractCqrs.Core.EventStore;

namespace AbstractCqrs.Core.Test.TestData
{
    public class EventRecord : IEventRecord
    {
        public Guid RootId { get; set; }

        public DateTime EventTimestamp { get; set; }

        public string Data { get; set; }
    }
}