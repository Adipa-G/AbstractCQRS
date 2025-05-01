using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Resolve.Helpers
{
    public class ResolveUtils
    {
        public static IList<CommandToEntityMapping> GetCommandToEntityMappings(
            Assembly[] assemblies)
        {
            var mappings = new List<CommandToEntityMapping>();

            var allHandlers = assemblies.SelectMany(
                a => a.GetTypes()
                    .Where(t => t.GetTypeInfo().GetInterfaces().Any(
                               i => i.GenericTypeArguments.Length > 0 &&
                                    i.GetGenericTypeDefinition() ==
                                    typeof(ICommandHandler<,>))));

            var allCmds = assemblies.SelectMany(
                a => a.GetTypes()
                    .Where(t => t.GetTypeInfo().GetInterfaces()
                               .Contains(typeof(ICommand)))).ToList();

            foreach (var handlerType in allHandlers)
            {
                var handlerInterfaceType = handlerType.GetTypeInfo()
                    .GetInterfaces().Single(
                        i => i.GenericTypeArguments.Length > 0 &&
                             i.GetGenericTypeDefinition() ==
                             typeof(ICommandHandler<,>));

                foreach (var cmdType in allCmds)
                    if (handlerInterfaceType.GenericTypeArguments[1] == cmdType)
                        mappings.Add(new CommandToEntityMapping(cmdType,
                            handlerInterfaceType.GenericTypeArguments[0]));
            }

            return mappings;
        }

        public static IList<CommandHandlerMapping> GetCommandHandlerMappings(
            Assembly[] assemblies)
        {
            var mappings = new List<CommandHandlerMapping>();

            var allHandlers = assemblies.SelectMany(
                a => a.GetTypes()
                    .Where(t => t.GetTypeInfo().GetInterfaces().Any(
                               i => i.GenericTypeArguments.Length > 0 &&
                                    i.GetGenericTypeDefinition() ==
                                    typeof(ICommandHandler<,>))));

            foreach (var handlerType in allHandlers)
            {
                var handlerInterfaceType = handlerType.GetTypeInfo()
                    .GetInterfaces().Single(
                        i => i.GenericTypeArguments.Length > 0 &&
                             i.GetGenericTypeDefinition() ==
                             typeof(ICommandHandler<,>));

                mappings.Add(new CommandHandlerMapping(
                    handlerInterfaceType.GenericTypeArguments[0],
                    handlerInterfaceType.GenericTypeArguments[1],
                    handlerType));
            }

            return mappings;
        }

        public static IList<EventHandlerMapping> GetEventHandlerMappings(
            Assembly[] assemblies)
        {
            var mappings = new List<EventHandlerMapping>();

            var allHandlers = assemblies.SelectMany(
                a => a.GetTypes()
                    .Where(t => t.GetTypeInfo().GetInterfaces().Any(
                               i => i.GenericTypeArguments.Length > 0 &&
                                    i.GetGenericTypeDefinition() ==
                                    typeof(IEventHandler<,>))));

            foreach (var handlerType in allHandlers)
            {
                var handlerInterfaceType = handlerType.GetTypeInfo()
                    .GetInterfaces().Single(
                        i => i.GenericTypeArguments.Length > 0 &&
                             i.GetGenericTypeDefinition() ==
                             typeof(IEventHandler<,>));

                mappings.Add(new EventHandlerMapping(
                    handlerInterfaceType.GenericTypeArguments[0],
                    handlerInterfaceType.GenericTypeArguments[1],
                    handlerType));
            }

            return mappings;
        }

        public static IList<Dependency> GetDependencies(Assembly[] assemblies)
        {
            var dependencies = new List<Dependency>();

            var classes = assemblies.SelectMany(
                a => a.GetTypes()
                    .Where(t => t.GetTypeInfo()
                                    .GetCustomAttribute<
                                        ServiceRegistrationAttribute
                                    >() != null));

            foreach (var @class in classes)
            {
                if (@class.IsAbstract)
                    continue;

                var interfaces = new List<Type>();
                var attribute = @class.GetTypeInfo()
                    .GetCustomAttribute<ServiceRegistrationAttribute>();

                var curClass = @class;
                while (curClass != typeof(object))
                {
                    interfaces.AddRange(curClass.GetInterfaces());
                    curClass = curClass.BaseType;
                }
                interfaces = interfaces.Distinct().ToList();

                foreach (var @interface in interfaces)
                    if (dependencies.All(d => d.Interface != @interface))
                        dependencies.Add(new Dependency(@interface, @class,
                            attribute.RegistrationType));
            }

            return dependencies;
        }
    }
}