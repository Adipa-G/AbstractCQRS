using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Order.Commands;
using AbstractCqrs.Sample.Domain.Order.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Order.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Commands.Handlers
{
    [TestFixture]
    public class CreateOrderCommandHandlerTest
    {
        [Test]
        public void GivenExistingOrder_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.NewGuid() };
            var cmd =
                new CreateOrderCommand(root.Id, "test", "1", 10);

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("already exists"));
        }

        [Test]
        public void GivenNonExistingOrder_WhenHandleCommand_ThenReturnEvents()
        {
            var root = new Order { Id = Guid.Empty };
            var cmd =
                new CreateOrderCommand(root.Id, "test", "1", 10);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as OrderCreatedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
            Assert.AreEqual(cmd.CustomerNotes, evt.CustomerNotes);
            Assert.AreEqual(cmd.OrderNo, evt.OrderNo);
            Assert.AreEqual(cmd.Discount, evt.Discount);
        }

        private CreateOrderCommandHandler CreateSut()
        {
            return new CreateOrderCommandHandler();
        }
    }
}