﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DistCqrs.Core.Domain;
using DistCqrs.Core.EventStore;
using DistCqrs.Core.Exceptions;
using DistCqrs.Core.Resolve;
using DistCqrs.Core.View;

namespace DistCqrs.Core.Command.Impl
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IRootTypeResolver rootTypeResolver;
        private readonly IEventStore eventStore;
        private readonly IViewWriter viewWriter;

        public CommandProcessor(IServiceLocator serviceLocator,
            IRootTypeResolver rootTypeResolver,
            IEventStore eventStore,
            IViewWriter viewWriter)
        {
            this.serviceLocator = serviceLocator;
            this.rootTypeResolver = rootTypeResolver;
            this.eventStore = eventStore;
            this.viewWriter = viewWriter;
        }

        public async Task Process(ICommand cmd)
        {
            var rootType = await eventStore.GetRootType(cmd.RootId) ??
                           rootTypeResolver.GetRootType(cmd);

            var commandHandler = InvokeGeneric<object>(this,
                "ResolveCommandHandler", new[] {rootType, cmd.GetType()});
            if (commandHandler == null)
            {
                throw new ServiceLocationException(
                    $"Cannot resolve service to process command of type {cmd.GetType().FullName}");
            }

            var root = await GetRoot(rootType,cmd.RootId);

            IList events = (IList)await Invoke<dynamic>(commandHandler, "Handle",
                new object[] { root, cmd });
            
            await InvokeGeneric<Task>(this, "SaveEvents",
                new[] {rootType}, new object[] {events});

            await ApplyEvents(root, events);

            await viewWriter.UpdateView(root);
        }

        private async Task<IRoot> GetRoot(Type rootType,Guid rootId)
        {
            var root = (IRoot)Activator.CreateInstance(rootType);
            var events = (IList)await InvokeGeneric<dynamic>(this,
                "GetEvents", new[] { rootType }, new object[] { rootId });

            if (events.Count == 0)
                return (IRoot)Activator.CreateInstance(rootType);

            await ApplyEvents(root, events);
            return root;
        }

        private async Task ApplyEvents<TRoot>(TRoot root, IList events)
            where TRoot : IRoot
        {
            foreach (var evt in events)
            {
                var evtHandler = InvokeGeneric<object>(this,
                    "ResolveEventHandler",
                    new[] {root.GetType(), evt.GetType()});
                var applyMethod = evtHandler.GetType().GetMethod("Apply");
                
                var task = (Task)applyMethod.Invoke(evtHandler, new[] { root, evt });
                await task;
            }
        }

        private T InvokeGeneric<T>(object src,
            string methodName,
            Type[] types,
            object[] values = null)
        {
            var method = src.GetType()
                .GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(types);

            return (T)genericMethod.Invoke(src, values);
        }

        private T Invoke<T>(object src,
            string methodName,
            object[] values = null)
        {
            var method = src.GetType().GetMethod(methodName);

            return (T)method.Invoke(src, values);
        }

        //wrappers to make refactor safe
        //ReSharper disable UnusedMember.Local
        ICommandHandler<TRoot, TCmd> ResolveCommandHandler<TRoot, TCmd>()
            where TRoot : IRoot, new()
            where TCmd : ICommand
        {
            return serviceLocator.ResolveCommandHandler<TRoot, TCmd>();
        }

        IEventHandler<TRoot, TEvent> ResolveEventHandler<TRoot, TEvent>()
            where TRoot : IRoot, new()
            where TEvent : IEvent<TRoot>
        {
            return serviceLocator.ResolveEventHandler<TRoot, TEvent>();
        }

        Task<IList<IEvent<TRoot>>> GetEvents<TRoot>(Guid rootId)
            where TRoot : IRoot
        {
            return eventStore.GetEvents<TRoot>(rootId);
        }

        Task SaveEvents<TRoot>(IList<IEvent<TRoot>> events)
            where TRoot : IRoot
        {
            return eventStore.SaveEvents(events);
        }
        //ReSharper restore UnusedMember.Local
    }
}