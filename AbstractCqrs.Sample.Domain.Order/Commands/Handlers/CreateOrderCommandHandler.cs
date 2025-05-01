using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Order.Events;

namespace AbstractCqrs.Sample.Domain.Order.Commands.Handlers
{
    public class CreateOrderCommandHandler : ICommandHandler<Order,
        CreateOrderCommand>
    {
        public Task<IList<IEvent<Order>>> Handle(Order root,
            CreateOrderCommand cmd)
        {
            IList<IEvent<Order>> list = new List<IEvent<Order>>();

            if (root.Id != Guid.Empty)
            {
                throw new DomainException($"Order {root.Id} already exists.");
            }

            list.Add(new OrderCreatedEvent(cmd.RootId, cmd.CustomerNotes,
                cmd.OrderNo, cmd.Discount));

            return Task.FromResult(list);
        }
    }
}