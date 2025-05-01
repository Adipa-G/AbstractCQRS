using System;

namespace AbstractCqrs.Sample.Domain.Order.Commands
{
    public class RemoveOrderItemCommand : BaseCommand
    {
        public RemoveOrderItemCommand(Guid rootId,
            Guid orderItemId) : base(rootId)
        {
            OrderItemId = orderItemId;
        }

        public Guid OrderItemId { get; set; }
    }
}