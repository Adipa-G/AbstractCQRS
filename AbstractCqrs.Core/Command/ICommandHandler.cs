using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Command
{
    public interface ICommandHandler<TRoot, in TCmd>
        where TRoot : IRoot
        where TCmd : ICommand
    {
        Task<IList<IEvent<TRoot>>> Handle(TRoot root, TCmd cmd);
    }
}