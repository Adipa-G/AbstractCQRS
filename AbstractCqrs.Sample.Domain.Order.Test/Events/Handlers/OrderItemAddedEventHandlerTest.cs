using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Order.Events;
using AbstractCqrs.Sample.Domain.Order.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Events.Handlers
{
    [TestFixture]
    public class OrderItemAddedEventHandlerTest
    {
        [Test]
        public void GivenOrderAndEvent_WhenApply_ThenAddItemAndUpdateTotal()
        {
            var root = new Order { Id = Guid.NewGuid(), Discount = 10 };

            var evt = new OrderItemAddedEvent(root.Id, Guid.NewGuid(), 1,
                Guid.NewGuid(), "test", 10, "Kg", 100);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(1, root.Items.Count);

            var orderItem = root.Items.First();
            Assert.AreEqual(evt.Amount, orderItem.Amount);
            Assert.AreEqual(evt.OrderItemId, orderItem.Id);
            Assert.AreEqual(evt.Ordinal, orderItem.Ordinal);
            Assert.AreEqual(evt.ProductId, orderItem.ProductId);
            Assert.AreEqual(evt.ProductText, orderItem.ProductText);
            Assert.AreEqual(evt.Qty, orderItem.Qty);
            Assert.AreEqual(evt.QtyUnit, orderItem.QtyUnit);

            Assert.AreEqual(90, root.Total);
        }

        private OrderItemAddedEventHandler CreateSut()
        {
            return new OrderItemAddedEventHandler();
        }
    }
}