using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Test.TestData
{
    public class AccountBalanceUpdatedEventHandler : IEventHandler<Account,
        AccountBalanceUpdatedEvent>
    {
        public Task Apply(Account root, AccountBalanceUpdatedEvent evt)
        {
            root.Balance += evt.Change;
            return Task.Delay(0);
        }
    }
}