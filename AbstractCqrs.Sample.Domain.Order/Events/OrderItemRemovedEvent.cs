using System;

namespace AbstractCqrs.Sample.Domain.Order.Events
{
    public class OrderItemRemovedEvent : BaseEvent<Order>
    {
        public OrderItemRemovedEvent(Guid rootId,
            Guid orderItemId) : base(rootId)
        {
            OrderItemId = orderItemId;
        }

        public Guid OrderItemId { get; set; }
    }
}