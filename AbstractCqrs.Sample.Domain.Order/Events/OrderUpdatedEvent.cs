using System;

namespace AbstractCqrs.Sample.Domain.Order.Events
{
    public class OrderUpdatedEvent : BaseEvent<Order>
    {
        public OrderUpdatedEvent(Guid rootId,
            string customerNotes,
            double discount) : base(rootId)
        {
            CustomerNotes = customerNotes;
            Discount = discount;
        }

        public string CustomerNotes { get; set; }

        public double Discount { get; set; }
    }
}