using System;

namespace AbstractCqrs.Sample.Domain.Order.Commands
{
    public class AddOrderItemCommand : BaseCommand
    {
        public AddOrderItemCommand(Guid rootId,
            Guid orderItemId,
            int ordinal,
            Guid productId,
            string productText,
            int qty,
            string qtyUnit,
            double amount) : base(rootId)
        {
            OrderItemId = orderItemId;
            Ordinal = ordinal;
            ProductId = productId;
            ProductText = productText;
            Qty = qty;
            QtyUnit = qtyUnit;
            Amount = amount;
        }

        public Guid OrderItemId { get; set; }

        public int Ordinal { get; set; }

        public Guid ProductId { get; set; }

        public string ProductText { get; set; }

        public int Qty { get; set; }

        public string QtyUnit { get; set; }

        public double Amount { get; set; }
    }
}