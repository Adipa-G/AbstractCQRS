using System;

namespace AbstractCqrs.Sample.Domain.Order.Commands
{
    public class UpdateOrderItemCommand : BaseCommand
    {
        public UpdateOrderItemCommand(Guid rootId,
            Guid orderItemId,
            int ordinal,
            string productText,
            int qty,
            double amount) : base(rootId)
        {
            OrderItemId = orderItemId;
            Ordinal = ordinal;
            ProductText = productText;
            Qty = qty;
            Amount = amount;
        }

        public Guid OrderItemId { get; set; }

        public int Ordinal { get; set; }

        public string ProductText { get; set; }

        public int Qty { get; set; }

        public double Amount { get; set; }
    }
}