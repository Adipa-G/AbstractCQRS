using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Order.Commands;
using AbstractCqrs.Sample.Domain.Order.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Order.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Order.Test.Commands.Handlers
{
    [TestFixture]
    public class DeleteOrderCommandHandlerTest
    {
        [Test]
        public void GivenNonExistingOrder_WhenHandleCommand_ThenThrowException()
        {
            var root = new Order { Id = Guid.Empty };
            var cmd =
                new DeleteOrderCommand(Guid.NewGuid());

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower()
                .Contains("does not exists"));
        }

        [Test]
        public void GivenDeletedOrder_WhenHandleCommand_ThenReturnEmpty()
        {
            var root = new Order { Id = Guid.NewGuid(), IsDeleted = true };
            var cmd =
                new DeleteOrderCommand(root.Id);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GivenExistingOrder_WhenHandleCommand_ThenReturnEvents()
        {
            var root = new Order { Id = Guid.NewGuid() };
            var cmd =
                new DeleteOrderCommand(root.Id);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as OrderDeletedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
        }

        private DeleteOrderCommandHandler CreateSut()
        {
            return new DeleteOrderCommandHandler();
        }
    }
}