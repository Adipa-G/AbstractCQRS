using System;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Test.TestData
{
    public class AccountBalanceUpdatedEvent : IEvent<Account>
    {
        public double Change { get; set; }
        public Guid RootId { get; set; }
    }
}