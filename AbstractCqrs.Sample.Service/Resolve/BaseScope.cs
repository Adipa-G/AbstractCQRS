using System;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Services;

namespace AbstractCqrs.Sample.Service.Resolve
{
    public abstract class BaseScope : IScope
    {
        private readonly IServiceLocator serviceLocator;

        protected BaseScope(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public IBus ResolveBus(string busId)
        {
            return serviceLocator.ResolveBus(busId);
        }

        public ICommandHandler<TRoot, TCmd> ResolveCommandHandler<TRoot, TCmd>()
            where TRoot : IRoot, new() where TCmd : ICommand
        {
            return (ICommandHandler<TRoot, TCmd>)Resolve(
                typeof(ICommandHandler<TRoot, TCmd>));
        }

        public IEventHandler<TRoot, TEvent> ResolveEventHandler<TRoot, TEvent>()
            where TRoot : IRoot, new() where TEvent : IEvent<TRoot>
        {
            return (IEventHandler<TRoot, TEvent>)Resolve(
                typeof(IEventHandler<TRoot, TEvent>));
        }

        public abstract void Dispose();

        protected abstract object Resolve(Type @interface);
    }
}