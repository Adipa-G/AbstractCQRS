using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Product.Commands;
using AbstractCqrs.Sample.Domain.Product.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Product.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Product.Test.Commands.Handlers
{
    [TestFixture]
    public class DeleteProductCommandHandlerTest
    {
        [Test]
        public void GivenDeletedProduct_WhenHandleCommand_ThenReturnEmpty()
        {
            var root = new Product { Id = Guid.NewGuid(), IsDeleted = true };
            var cmd =
                new DeleteProductCommand(root.Id);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GivenNonExistantProduct_WhenHandleCommand_ThenThrowException()
        {
            var root = new Product { Id = Guid.Empty };
            var cmd =
                new DeleteProductCommand(Guid.NewGuid());

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower().Contains("does not exist"));
        }

        [Test]
        public void GivenExistingProduct_WhenHandleCommand_ThenReturnEvents()
        {
            var root = new Product { Id = Guid.NewGuid() };
            var cmd =
                new DeleteProductCommand(root.Id);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as ProductDeletedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
        }

        private DeleteProductCommandHandler CreateSut()
        {
            return new DeleteProductCommandHandler();
        }
    }
}