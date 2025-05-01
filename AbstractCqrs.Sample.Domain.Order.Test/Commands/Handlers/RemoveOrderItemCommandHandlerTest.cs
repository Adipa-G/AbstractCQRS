using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Order.Commands;
using AbstractCqrs.Sample.Domain.Order.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Order.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Commands.Handlers
{
    [TestFixture]
    public class RemoveOrderItemCommandHandlerTest
    {
        [Test]
        public void GivenNonExistingOrder_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.Empty };
            var cmd =
                new RemoveOrderItemCommand(Guid.NewGuid(), Guid.NewGuid());

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("does not exists"));
        }

        [Test]
        public void GivenNonExistingOrderItem_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.NewGuid() };

            var cmd =
                new RemoveOrderItemCommand(root.Id, Guid.NewGuid());

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("does not exists"));
        }

        [Test]
        public void GivenDeletedOrder_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.NewGuid(), IsDeleted = true };
            var orderItem = new OrderItem { Id = Guid.NewGuid() };
            root.Items.Add(orderItem);

            var cmd =
                new RemoveOrderItemCommand(root.Id, orderItem.Id);

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("deleted"));
        }

        [Test]
        public void GivenExistingOrderItem_WhenHandleCommand_ThenReturnEvents()
        {
            var root = new Order { Id = Guid.NewGuid() };
            var orderItem = new OrderItem { Id = Guid.NewGuid() };
            root.Items.Add(orderItem);

            var cmd =
                new RemoveOrderItemCommand(root.Id, orderItem.Id);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as OrderItemRemovedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
            Assert.AreEqual(cmd.OrderItemId, evt.OrderItemId);
        }


        private RemoveOrderItemCommandHandler CreateSut()
        {
            return new RemoveOrderItemCommandHandler();
        }
    }
}