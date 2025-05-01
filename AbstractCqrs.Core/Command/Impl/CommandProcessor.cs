using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.EventStore;
using AbstractCqrs.Core.Exceptions;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Services;
using AbstractCqrs.Core.View;

namespace AbstractCqrs.Core.Command.Impl
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class CommandProcessor : ICommandProcessor
    {
        private static readonly
            ConcurrentDictionary<string, MethodInfo> MethodCache =
                new ConcurrentDictionary<string, MethodInfo>();

        private readonly IEventStore eventStore;
        private readonly IRootTypeResolver rootTypeResolver;
        private readonly IServiceLocator serviceLocator;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IViewWriter viewWriter;
        private readonly ILog log;

        public CommandProcessor(IServiceLocator serviceLocator,
            IRootTypeResolver rootTypeResolver,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventStore eventStore,
            IViewWriter viewWriter,
            ILog log)
        {
            this.serviceLocator = serviceLocator;
            this.rootTypeResolver = rootTypeResolver;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.eventStore = eventStore;
            this.viewWriter = viewWriter;
            this.log = log;
        }

        public async Task Process(ICommand cmd)
        {
            var error = false;
            var logBuilder = new StringBuilder();

            try
            {
                IRoot root;

                logBuilder.AppendLine($"Start command {cmd.GetType().FullName} {cmd.RootId}");
                logBuilder.AppendLine($"\tStart processing command");

                using (var scope = serviceLocator.CreateScope())
                using (var unitOfWork = unitOfWorkFactory.Create())
                {
                    var rootType = await eventStore.GetRootType(cmd.RootId) ??
                                   rootTypeResolver.GetRootType(cmd);
                    logBuilder.AppendLine($"\t\tFound Root Type {rootType.FullName}");

                    var commandHandler = InvokeGeneric<object>(this,
                        "ResolveCommandHandler", new[] { rootType, cmd.GetType() },
                        new object[] { scope });

                    if (commandHandler == null)
                    {
                        var errorMsg =
                            $"Cannot resolve service to process command of type {cmd.GetType().FullName}";

                        logBuilder.AppendLine($"\t\t{errorMsg}");
                        throw new ServiceLocationException(errorMsg);
                    }
                    logBuilder.AppendLine(
                        $"\t\tFound command handler {commandHandler.GetType().FullName}");

                    root = await GetRoot(scope, rootType, cmd.RootId);
                    logBuilder.AppendLine($"\t\tLoaded root {root.GetType().FullName}");

                    var events = (IList)await Invoke<dynamic>(commandHandler,
                        "Handle",
                        new object[] { root, cmd });
                    logBuilder.AppendLine($"\t\tGenerated {events.Count} events");

                    await InvokeGeneric<Task>(this, "SaveEvents",
                        new[] { rootType }, new object[] { events });
                    logBuilder.AppendLine("\t\tSaved events");

                    await ApplyEvents(scope, root, events);
                    logBuilder.AppendLine("\t\tApplied events");

                    await unitOfWork.Complete();
                    logBuilder.AppendLine("\t\tUnit of work completed");
                }
                logBuilder.AppendLine("\tCompleted processing command");

                logBuilder.AppendLine($"\tStart updating view {cmd.RootId}");
                await viewWriter.UpdateView(root);
                logBuilder.AppendLine($"\tCompleted updating view {cmd.RootId}");
            }
            catch (Exception ex)
            {
                error = true;
                logBuilder.AppendLine(
                    $"\tException while processing command {ex}");
                throw;
            }
            finally
            {
                logBuilder.AppendLine($"End command {cmd.GetType().FullName} {cmd.RootId}");
                if (error)
                {
                    log.LogError(logBuilder.ToString());
                }
                else
                {
                    log.LogDebug(logBuilder.ToString());
                }
            }
        }

        private async Task<IRoot> GetRoot(IScope scope, Type rootType, Guid rootId)
        {
            var root = (IRoot)Activator.CreateInstance(rootType);
            var events = (IList)await InvokeGeneric<dynamic>(this,
                "GetEvents", new[] { rootType }, new object[] { rootId });

            if (events.Count == 0)
                return (IRoot)Activator.CreateInstance(rootType);

            await ApplyEvents(scope, root, events);
            return root;
        }

        private async Task ApplyEvents<TRoot>(IScope scope, TRoot root, IList events)
            where TRoot : IRoot
        {
            foreach (var evt in events)
            {
                var evtHandler = InvokeGeneric<object>(this,
                    "ResolveEventHandler",
                    new[] { root.GetType(), evt.GetType() },
                    new object[] { scope });
                var applyMethod = evtHandler.GetType().GetTypeInfo()
                    .GetMethod("Apply");

                var task =
                    (Task)applyMethod.Invoke(evtHandler, new[] { root, evt });
                await task;
            }
        }

        private T InvokeGeneric<T>(object src,
            string methodName,
            Type[] types,
            object[] values = null)
        {
            var cacheKey = $"{src.GetType().GetTypeInfo().FullName}_" +
                           $"{methodName}_" +
                           $"{string.Join(",", types.Select(t => t.FullName))}";

            MethodInfo genericMethod;
            if (MethodCache.ContainsKey(cacheKey))
            {
                genericMethod = MethodCache[cacheKey];
            }
            else
            {
                var method = src.GetType().GetTypeInfo().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
                genericMethod = method.MakeGenericMethod(types);

                MethodCache.TryAdd(cacheKey, genericMethod);
            }

            return (T)genericMethod.Invoke(src, values);
        }

        private T Invoke<T>(object src,
            string methodName,
            object[] values = null)
        {
            var cacheKey = $"{src.GetType().GetTypeInfo().FullName}_" +
                           $"{methodName}";

            MethodInfo method;
            if (MethodCache.ContainsKey(cacheKey))
            {
                method = MethodCache[cacheKey];
            }
            else
            {
                method = src.GetType().GetTypeInfo().GetMethod(methodName);
                MethodCache.TryAdd(cacheKey, method);
            }

            return (T)method.Invoke(src, values);
        }

        //wrappers to make refactor safe
        //ReSharper disable UnusedMember.Local
        private ICommandHandler<TRoot, TCmd>
            ResolveCommandHandler<TRoot, TCmd>(IScope scope)
            where TRoot : IRoot, new()
            where TCmd : ICommand
        {
            return scope.ResolveCommandHandler<TRoot, TCmd>();
        }

        private IEventHandler<TRoot, TEvent>
            ResolveEventHandler<TRoot, TEvent>(IScope scope)
            where TRoot : IRoot, new()
            where TEvent : IEvent<TRoot>
        {
            return scope.ResolveEventHandler<TRoot, TEvent>();
        }

        private Task<IList<IEvent<TRoot>>> GetEvents<TRoot>(Guid rootId)
            where TRoot : IRoot
        {
            return eventStore.GetEvents<TRoot>(rootId);
        }

        private Task SaveEvents<TRoot>(IList<IEvent<TRoot>> events)
            where TRoot : IRoot
        {
            return eventStore.SaveEvents(events);
        }
        //ReSharper restore UnusedMember.Local
    }
}