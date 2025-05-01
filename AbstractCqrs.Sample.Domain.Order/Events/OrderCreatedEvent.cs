using System;

namespace AbstractCqrs.Sample.Domain.Order.Events
{
    public class OrderCreatedEvent : BaseEvent<Order>
    {
        public OrderCreatedEvent(Guid rootId,
            string customerNotes,
            string orderNo,
            double discount) : base(rootId)
        {
            CustomerNotes = customerNotes;
            OrderNo = orderNo;
            Discount = discount;
        }

        public string CustomerNotes { get; set; }

        public string OrderNo { get; set; }

        public double Discount { get; set; }
    }
}