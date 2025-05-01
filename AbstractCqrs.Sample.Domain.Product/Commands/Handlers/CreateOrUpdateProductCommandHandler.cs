using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Product.Events;

namespace AbstractCqrs.Sample.Domain.Product.Commands.Handlers
{
    public class CreateOrUpdateProductCommandHandler : ICommandHandler<Product,
        CreateOrUpdateProductCommand>
    {
        public Task<IList<IEvent<Product>>> Handle(Product root,
            CreateOrUpdateProductCommand cmd)
        {
            IList<IEvent<Product>> list = new List<IEvent<Product>>();
            if (root.Id == Guid.Empty)
            {
                list.Add(new ProductCreatedEvent(cmd.RootId, cmd.Code, cmd.Name,
                    cmd.UnitPrice));
            }
            else
            {
                if (root.IsDeleted)
                {
                    throw new DomainException($"Product {cmd.RootId} is deleted.");
                }

                list.Add(new ProductUpdatedEvent(cmd.RootId, cmd.Code, cmd.Name,
                    cmd.UnitPrice));
            }
            return Task.FromResult(list);
        }
    }
}