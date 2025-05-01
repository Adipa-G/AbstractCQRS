using System;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Services;

namespace AbstractCqrs.Core.Resolve
{
    public interface IScope : IDisposable
    {
        T Resolve<T>();

        IBus ResolveBus(string busId);

        ICommandHandler<TRoot, TCmd> ResolveCommandHandler<TRoot, TCmd>()
            where TRoot : IRoot, new()
            where TCmd : ICommand;

        IEventHandler<TRoot, TEvent> ResolveEventHandler<TRoot, TEvent>()
            where TRoot : IRoot, new()
            where TEvent : IEvent<TRoot>;
    }
}