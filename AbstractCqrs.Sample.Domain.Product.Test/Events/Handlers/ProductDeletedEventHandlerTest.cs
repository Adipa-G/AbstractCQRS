using System;

using AbstractCqrs.Sample.Domain.Product.Events;
using AbstractCqrs.Sample.Domain.Product.Events.Handlers;

using NUnit.Framework;

namespace AbstractCqrs.Sample.Domain.Product.Test.Events.Handlers
{
    [TestFixture]
    public class ProductDeletedEventHandlerTest
    {
        [Test]
        public void GivenProductAndEvent_WhenApply_ThenDeleteProduct()
        {
            var root = new Product { Id = Guid.NewGuid() };
            var evt = new ProductDeletedEvent(root.Id);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.IsTrue(root.IsDeleted);
        }

        private ProductDeletedEventHandler CreateSut()
        {
            return new ProductDeletedEventHandler();
        }
    }
}