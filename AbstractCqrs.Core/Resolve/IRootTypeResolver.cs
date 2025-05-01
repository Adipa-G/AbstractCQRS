using System;

using AbstractCqrs.Core.Command;

namespace AbstractCqrs.Core.Resolve
{
    public interface IRootTypeResolver
    {
        Type GetRootType(ICommand cmd);
    }
}