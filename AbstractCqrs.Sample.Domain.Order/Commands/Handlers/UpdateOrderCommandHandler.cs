using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Order.Events;

namespace AbstractCqrs.Sample.Domain.Order.Commands.Handlers
{
    public class UpdateOrderCommandHandler : ICommandHandler<Order,
        UpdateOrderCommand>
    {
        public Task<IList<IEvent<Order>>> Handle(Order root,
            UpdateOrderCommand cmd)
        {
            IList<IEvent<Order>> list = new List<IEvent<Order>>();

            if (root.Id == Guid.Empty)
            {
                throw new DomainException($"Order {root.Id} does not exists.");
            }

            if (root.IsDeleted)
            {
                throw new DomainException($"Order {root.Id} is deleted.");
            }

            list.Add(new OrderUpdatedEvent(cmd.RootId, cmd.CustomerNotes,
                cmd.Discount));

            return Task.FromResult(list);
        }
    }
}