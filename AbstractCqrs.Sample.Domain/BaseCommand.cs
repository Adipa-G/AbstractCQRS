using System;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Services;

namespace AbstractCqrs.Sample.Domain
{
    public abstract class BaseCommand : ICommand, IBusMessage
    {
        protected BaseCommand(Guid rootId)
        {
            RootId = rootId;
        }

        public Guid RootId { get; }
    }
}