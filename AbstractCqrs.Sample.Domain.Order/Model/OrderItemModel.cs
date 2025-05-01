using System;

namespace AbstractCqrs.Sample.Domain.Order.Model
{
    public class OrderItemModel
    {
        public Guid Id { get; set; }

        public int Ordinal { get; set; }

        public Guid ProductId { get; set; }

        public string ProductText { get; set; }

        public int Qty { get; set; }

        public string QtyUnit { get; set; }

        public double Amount { get; set; }
    }
}