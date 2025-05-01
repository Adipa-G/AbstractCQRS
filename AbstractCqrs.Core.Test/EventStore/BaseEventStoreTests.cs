using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Test.TestData;

using NUnit.Framework;

namespace AbstractCqrs.Core.Test.EventStore
{
    [TestFixture]
    public class BaseEventStoreTests
    {
        private InMemoryEventStore CreateSut()
        {
            return new InMemoryEventStore();
        }

        [Test]
        public async Task GivenEvents_WhenSaveEvents_ThenSave()
        {
            var accountId = Guid.NewGuid();
            var events = new List<IEvent<Account>>
                         {
                             new AccountCreatedEvent {RootId = accountId},
                             new AccountBalanceUpdatedEvent
                             {
                                 Change = 100,
                                 RootId = accountId
                             }
                         };

            var sut = CreateSut();
            await sut.SaveEvents(events);

            Assert.AreEqual(2, sut.EventRecords.Count);
            Assert.IsTrue(sut.EventRecords.All(er => er.RootId == accountId));
            Assert.IsTrue(sut.EventRecords.All(er => er.Data != null));
        }

        [Test]
        public async Task GivenSavedEvents_WhenLoadEvents_ThenLoad()
        {
            var accountId = Guid.NewGuid();
            var events = new List<IEvent<Account>>
                         {
                             new AccountCreatedEvent {RootId = accountId},
                             new AccountBalanceUpdatedEvent
                             {
                                 Change = 100,
                                 RootId = accountId
                             }
                         };

            var sut = CreateSut();
            await sut.SaveEvents(events);

            var loaded = await sut.GetEvents<Account>(accountId);

            Assert.AreEqual(events.Count, loaded.Count);

            var created = loaded[0] as AccountCreatedEvent;
            Assert.IsNotNull(created);
            Assert.AreEqual(accountId, created.RootId);

            var updated = loaded[1] as AccountBalanceUpdatedEvent;
            Assert.IsNotNull(updated);
            Assert.AreEqual(accountId, updated.RootId);
            Assert.AreEqual(100, updated.Change);
        }
    }
}