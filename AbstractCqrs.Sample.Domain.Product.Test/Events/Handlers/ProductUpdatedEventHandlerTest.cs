using System;

using AbstractCqrs.Sample.Domain.Product.Events;
using AbstractCqrs.Sample.Domain.Product.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Product.Test.Events.Handlers
{
    [TestFixture]
    public class ProductUpdatedEventHandlerTest
    {
        [Test]
        public void GivenProductAndEvent_WhenApply_ThenUpdateProduct()
        {
            var root = new Product { Id = Guid.NewGuid() };
            var evt = new ProductUpdatedEvent(root.Id, "A", "B", 100);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(evt.Code, root.Code);
            Assert.AreEqual(evt.Name, root.Name);
            Assert.AreEqual(evt.UnitPrice, root.UnitPrice);
        }

        private ProductUpdatedEventHandler CreateSut()
        {
            return new ProductUpdatedEventHandler();
        }
    }
}