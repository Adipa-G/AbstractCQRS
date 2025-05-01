using System;

using AbstractCqrs.Sample.Domain.Order.Events;
using AbstractCqrs.Sample.Domain.Order.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Events.Handlers
{
    [TestFixture]
    public class OrderItemRemovedEventHandlerTest
    {
        [Test]
        public void GivenOrderItemAndEvent_WhenApply_ThenRemoveItemAndUpdateTotal()
        {
            var root = new Order { Id = Guid.NewGuid(), Discount = 10 };

            var item1 = new OrderItem
            {
                Id = Guid.NewGuid(),
                Ordinal = 1,
                Amount = 100
            };
            root.Items.Add(item1);

            var item2 = new OrderItem
            {
                Id = Guid.NewGuid(),
                Ordinal = 2,
                Amount = 100
            };
            root.Items.Add(item2);

            var evt = new OrderItemRemovedEvent(root.Id, item1.Id);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(1, root.Items.Count);
            Assert.AreEqual(90, root.Total);

            Assert.AreEqual(1, item2.Ordinal);
        }

        private OrderItemRemovedEventHandler CreateSut()
        {
            return new OrderItemRemovedEventHandler();
        }
    }
}