using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.EventStore;
using AbstractCqrs.Core.EventStore.Impl;
using AbstractCqrs.Core.Resolve;

using Newtonsoft.Json;

namespace AbstractCqrs.Core.Test.TestData
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class InMemoryEventStore : BaseEventStore
    {
        public InMemoryEventStore()
        {
            EventRecords = new List<IEventRecord>();
        }

        public List<IEventRecord> EventRecords { get; }

        protected override IEventRecord Create()
        {
            return new EventRecord();
        }

        protected override string Serialize<TRoot>(IEvent<TRoot> evt)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(evt, settings);
        }

        protected override IEvent<TRoot> DeSerialize<TRoot>(string data)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return (IEvent<TRoot>)JsonConvert.DeserializeObject(data,
                settings);
        }

        protected override async Task Save(IList<IEventRecord> records)
        {
            EventRecords.AddRange(records);
            await Task.Delay(0);
        }

        protected override async Task<IList<IEventRecord>> Load(Guid rootId)
        {
            IList<IEventRecord> events =
                EventRecords.Where(er => er.RootId == rootId).ToList();
            var result = await Task.FromResult(events);
            return result;
        }

        public override async Task<Type> GetRootType(Guid rootId)
        {
            var result = await Task.FromResult(typeof(Account));
            return result;
        }
    }
}