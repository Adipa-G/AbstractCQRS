using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Order.Commands;
using AbstractCqrs.Sample.Domain.Order.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Order.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Commands.Handlers
{
    [TestFixture]
    public class UpdateOrderCommandHandlerTest
    {
        [Test]
        public void GivenNonExistingOrder_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.Empty };
            var cmd =
                new UpdateOrderCommand(Guid.NewGuid(), "test", 10);

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
            var cmd =
                new UpdateOrderCommand(Guid.NewGuid(), "test", 10);

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("deleted"));
        }

        [Test]
        public void GivenExistingOrder_WhenHandleCommand_ThenReturnEvents()
        {
            var root = new Order { Id = Guid.NewGuid() };
            var cmd =
                new UpdateOrderCommand(root.Id, "test", 10);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as OrderUpdatedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
            Assert.AreEqual(cmd.CustomerNotes, evt.CustomerNotes);
            Assert.AreEqual(cmd.Discount, evt.Discount);
        }

        private UpdateOrderCommandHandler CreateSut()
        {
            return new UpdateOrderCommandHandler();
        }
    }
}