using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using AbstractCqrs.Core.Exceptions;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Services;

namespace AbstractCqrs.Sample.Service.Resolve
{
    public abstract class BaseServiceLocator : IServiceLocator
    {
        private readonly IDictionary<string, IBus> buses;

        protected BaseServiceLocator()
        {
            buses = new ConcurrentDictionary<string, IBus>();
        }

        public IBus ResolveBus(string busId)
        {
            if (!buses.ContainsKey(busId))
                throw new ServiceLocationException(
                    $"Bus {busId} is not registered.");
            return buses[busId];
        }

        public void Register(IBus bus)
        {
            if (buses.ContainsKey(bus.Id))
                throw new ServiceRegistrationException(
                    $"Bus {bus.Id} is already registered.");
            buses.Add(bus.Id, bus);
        }

        public abstract IScope CreateScope();
    }
}