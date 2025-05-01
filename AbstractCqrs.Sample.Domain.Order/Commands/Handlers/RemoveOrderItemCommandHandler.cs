using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Order.Events;

namespace AbstractCqrs.Sample.Domain.Order.Commands.Handlers
{
    public class
        RemoveOrderItemCommandHandler : ICommandHandler<Order, RemoveOrderItemCommand>
    {
        public Task<IList<IEvent<Order>>> Handle(Order root,
            RemoveOrderItemCommand cmd)
        {
            IList<IEvent<Order>> list = new List<IEvent<Order>>();

            if (root.Id == Guid.Empty)
            {
                throw new DomainException($"Order {root.Id} does not exists.");
            }

            if (root.Items.All(i => i.Id != cmd.OrderItemId))
            {
                throw new DomainException($"Order item {cmd.OrderItemId} does not exists.");
            }

            if (root.IsDeleted)
            {
                throw new DomainException($"Order {root.Id} is deleted.");
            }

            list.Add(new OrderItemRemovedEvent(cmd.RootId, cmd.OrderItemId));
            return Task.FromResult(list);
        }
    }
}