﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistCqrs.Core.Command;
using DistCqrs.Core.Domain;
using DistCqrs.Sample.Domain.Product.Events;

namespace DistCqrs.Sample.Domain.Product.Commands.Handlers
{
    public class
        DeleteProductCommandHandler : ICommandHandler<Product,
            DeleteProductCommand>
    {
        public Task<IList<IEvent<Product>>> Handle(Product root,
            DeleteProductCommand cmd)
        {
            IList<IEvent<Product>> list = new List<IEvent<Product>>();

            if (root.Id == Guid.Empty)
            {
                throw new DomainException($"Product {cmd.RootId} does not exist.");
            }

            if (root.IsDeleted)
            {
                return Task.FromResult(list);
            }
            
            list.Add(new ProductDeletedEvent(cmd.RootId));
            return Task.FromResult(list);
        }
    }
}