using System;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Test.TestData
{
    public class AccountCreatedEvent : IEvent<Account>
    {
        public Guid RootId { get; set; }
    }
}