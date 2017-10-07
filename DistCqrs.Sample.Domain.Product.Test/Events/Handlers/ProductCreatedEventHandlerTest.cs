﻿using System;
using DistCqrs.Sample.Domain.Product.Events;
using DistCqrs.Sample.Domain.Product.Events.Handlers;
using NUnit.Framework;

namespace DistCqrs.Sample.Domain.Product.Test.Events.Handlers
{
    [TestFixture]
    public class ProductCreatedEventHandlerTest
    {
        [Test]
        public void GivenProductAndEvent_WhenApply_ThenUpdateProduct()
        {
            var root = new Product {Id = Guid.NewGuid()};
            var evt = new ProductCreatedEvent(root.Id, "A", "B", 100);

            var sut = CreateSut();
            sut.Apply(root, evt).Wait();

            Assert.AreEqual(evt.Code, root.Code);
            Assert.AreEqual(evt.Name, root.Name);
            Assert.AreEqual(evt.UnitPrice, root.UnitPrice);
        }

        private ProductCreatedEventHandler CreateSut()
        {
            return new ProductCreatedEventHandler();
        }
    }
}
