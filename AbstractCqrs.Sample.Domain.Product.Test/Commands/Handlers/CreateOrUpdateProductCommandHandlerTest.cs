using System;
using System.Linq;

using AbstractCqrs.Sample.Domain.Product.Commands;
using AbstractCqrs.Sample.Domain.Product.Commands.Handlers;
using AbstractCqrs.Sample.Domain.Product.Events;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Product.Test.Commands.Handlers
{
    [TestFixture]
    public class CreateOrUpdateProductCommandHandlerTest
    {
        [Test]
        public void GivenNonExistingProduct_WhenHandleCreateCommand_ThenReturnEvents()
        {
            var root = new Product { Id = Guid.Empty };
            var cmd =
                new CreateOrUpdateProductCommand(Guid.NewGuid(), "X", "Y", 100);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as ProductCreatedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(cmd.RootId, evt.RootId);
            Assert.AreEqual("X", evt.Code);
            Assert.AreEqual("Y", evt.Name);
            Assert.AreEqual(100, evt.UnitPrice);
        }

        [Test]
        public void GivenDeletedProduct_WhenHandleUpdateCommand_ThenThrowError()
        {
            var root = new Product { Id = Guid.NewGuid(), IsDeleted = true };
            var cmd =
                new CreateOrUpdateProductCommand(root.Id, "X", "Y", 100);

            var sut = CreateSut();
            var exception =
                Assert.ThrowsAsync<DomainException>(
                    () => sut.Handle(root, cmd));
            Assert.IsTrue(exception.Message.ToLower().Contains("deleted"));
        }

        [Test]
        public void GivenExistingProduct_WhenHandleUpdateCommand_ThenReturnEvents()
        {
            var root = new Product { Id = Guid.NewGuid() };
            var cmd =
                new CreateOrUpdateProductCommand(root.Id, "X", "Y", 100);

            var sut = CreateSut();
            var task = sut.Handle(root, cmd);
            task.Wait();

            var result = task.Result;
            Assert.AreEqual(1, result.Count);

            var evt = result.First() as ProductUpdatedEvent;
            Assert.IsNotNull(evt);
            Assert.AreEqual(root.Id, evt.RootId);
            Assert.AreEqual("X", evt.Code);
            Assert.AreEqual("Y", evt.Name);
            Assert.AreEqual(100, evt.UnitPrice);
        }

        private CreateOrUpdateProductCommandHandler CreateSut()
        {
            return new CreateOrUpdateProductCommandHandler();
        }
    }
}