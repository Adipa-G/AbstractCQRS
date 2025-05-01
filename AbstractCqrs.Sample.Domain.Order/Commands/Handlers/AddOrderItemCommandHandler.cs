using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Order.Events;

namespace AbstractCqrs.Sample.Domain.Order.Commands.Handlers
{
    public class AddOrderItemCommandHandler : ICommandHandler<Order,
        AddOrderItemCommand>
    {
        public Task<IList<IEvent<Order>>> Handle(Order root,
            AddOrderItemCommand cmd)
        {
            IList<IEvent<Order>> list = new List<IEvent<Order>>();
            if (root.Id == Guid.Empty)
            {
                throw new DomainException($"Order {root.Id} does not exists.");
            }

            if (root.Items.Any(i => i.Id == cmd.OrderItemId))
            {
                throw new DomainException($"Order item {cmd.OrderItemId} already exists.");
            }

            if (root.IsDeleted)
            {
                throw new DomainException($"Order {root.Id} is deleted.");
            }

            list.Add(new OrderItemAddedEvent(cmd.RootId,
                cmd.OrderItemId,
                cmd.Ordinal,
                cmd.ProductId,
                cmd.ProductText,
                cmd.Qty,
                cmd.QtyUnit,
                cmd.Amount));
            return Task.FromResult(list);
        }
    }
}