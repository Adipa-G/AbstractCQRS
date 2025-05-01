using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Sample.Domain.Order.Events;

namespace AbstractCqrs.Sample.Domain.Order.Commands.Handlers
{
    public class
        DeleteOrderCommandHandler : ICommandHandler<Order, DeleteOrderCommand>
    {
        public Task<IList<IEvent<Order>>> Handle(Order root,
            DeleteOrderCommand cmd)
        {
            IList<IEvent<Order>> list = new List<IEvent<Order>>();

            if (root.Id == Guid.Empty)
            {
                throw new DomainException($"Order {root.Id} does not exists.");
            }

            if (root.IsDeleted)
            {
                return Task.FromResult(list);
            }

            list.Add(new OrderDeletedEvent(cmd.RootId));
            return Task.FromResult(list);
        }
    }
}