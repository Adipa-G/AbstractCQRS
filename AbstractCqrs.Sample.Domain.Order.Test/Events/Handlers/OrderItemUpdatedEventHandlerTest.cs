using System;

using AbstractCqrs.Sample.Domain.Order.Events;
using AbstractCqrs.Sample.Domain.Order.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Events.Handlers
{
    [TestFixture]
    public class OrderItemUpdatedEventHandlerTest
    {
        [Test]
        public void GivenOrderItemAndEvent_WhenApply_ThenUpdateItemAndUpdateTotal()
        {
            var root = new Order { Id = Guid.NewGuid(), Discount = 10 };

            var item1 = new OrderItem { Id = Guid.NewGuid(), Amount = 100 };
            root.Items.Add(item1);

            var item2 = new OrderItem { Id = Guid.NewGuid(), Amount = 100 };
            root.Items.Add(item2);

            var evt =
                new OrderItemUpdatedEvent(root.Id, item1.Id, 1, "A", 10, 50);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(evt.Ordinal, item1.Ordinal);
            Assert.AreEqual(evt.ProductText, item1.ProductText);
            Assert.AreEqual(evt.Qty, item1.Qty);
            Assert.AreEqual(evt.Amount, item1.Amount);

            Assert.AreEqual(140, root.Total);
        }

        private OrderItemUpdatedEventHandler CreateSut()
        {
            return new OrderItemUpdatedEventHandler();
        }
    }
}