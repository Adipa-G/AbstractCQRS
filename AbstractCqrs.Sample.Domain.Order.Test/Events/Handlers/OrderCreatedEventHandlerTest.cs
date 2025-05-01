using System;

using AbstractCqrs.Sample.Domain.Order.Events;
using AbstractCqrs.Sample.Domain.Order.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Events.Handlers
{
    [TestFixture]
    public class OrderCreatedEventHandlerTest
    {
        [Test]
        public void GivenOrderAndEvent_WhenApply_ThenUpdateOrder()
        {
            var root = new Order { Id = Guid.NewGuid() };
            var orderItem = new OrderItem { Amount = 100 };
            root.Items.Add(orderItem);

            var evt = new OrderCreatedEvent(root.Id, "A", "B", 10);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(evt.RootId, root.Id);
            Assert.AreEqual(evt.CustomerNotes, root.CustomerNotes);
            Assert.AreEqual(evt.OrderNo, root.OrderNo);
            Assert.AreEqual(evt.Discount, root.Discount);
            Assert.AreEqual(90, root.Total);
        }

        private OrderCreatedEventHandler CreateSut()
        {
            return new OrderCreatedEventHandler();
        }
    }
}