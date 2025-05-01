using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.EventStore.Impl
{
    public abstract class BaseEventStore : IEventStore
    {
        public abstract Task<Type> GetRootType(Guid rootId);

        public async Task SaveEvents<TRoot>(IList<IEvent<TRoot>> events)
            where TRoot : IRoot
        {
            var eventRecords = new List<IEventRecord>();
            foreach (var evt in events)
            {
                var record = Create();
                record.RootId = evt.RootId;
                record.EventTimestamp = DateTime.UtcNow;
                record.Data = Serialize(evt);
                eventRecords.Add(record);
            }
            await Save(eventRecords);
        }

        public async Task<IList<IEvent<TRoot>>> GetEvents<TRoot>(Guid rootId)
            where TRoot : IRoot
        {
            var events = new List<IEvent<TRoot>>();
            var eventRecords = await Load(rootId);
            var sortedRecords = eventRecords.OrderBy(r => r.EventTimestamp);

            foreach (var record in sortedRecords)
                events.Add(DeSerialize<TRoot>(record.Data));

            return events;
        }

        protected abstract IEventRecord Create();

        protected abstract string Serialize<TRoot>(IEvent<TRoot> evt)
            where TRoot : IRoot;

        protected abstract IEvent<TRoot> DeSerialize<TRoot>(string data)
            where TRoot : IRoot;

        protected abstract Task Save(IList<IEventRecord> records);

        protected abstract Task<IList<IEventRecord>> Load(Guid rootId);
    }
}