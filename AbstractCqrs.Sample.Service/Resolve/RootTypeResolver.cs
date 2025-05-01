using System;
using System.Collections.Generic;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Resolve.Helpers;

namespace AbstractCqrs.Sample.Service.Resolve
{
    [ServiceRegistration(ServiceRegistrationType.Singleton)]
    public class RootTypeResolver : IRootTypeResolver
    {
        private readonly IDictionary<Type, Type> mappings;

        public RootTypeResolver()
        {
            mappings = new Dictionary<Type, Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var commandToEntityMappings =
                ResolveUtils.GetCommandToEntityMappings(assemblies);

            foreach (var mapping in commandToEntityMappings)
                mappings.Add(mapping.CommandType, mapping.EntityType);
        }

        public Type GetRootType(ICommand cmd)
        {
            return mappings[cmd.GetType()];
        }
    }
}