using System;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain
{
    public abstract class BaseEvent<T> : IEvent<T> where T : IRoot
    {
        protected BaseEvent(Guid rootId)
        {
            RootId = rootId;
        }

        public Guid RootId { get; }
    }
}