using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Test.TestData
{
    public class
        CreateAccountCommandHandler : ICommandHandler<Account,
            CreateAccountCommand>
    {
        public Task<IList<IEvent<Account>>> Handle(Account root,
            CreateAccountCommand cmd)
        {
            IList<IEvent<Account>> list = new List<IEvent<Account>>();
            list.Add(new AccountCreatedEvent { RootId = cmd.RootId });
            return Task.FromResult(list);
        }
    }
}