using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Order.Events.Handlers
{
    public class OrderItemAddedEventHandler : IEventHandler<Order, OrderItemAddedEvent>
    {
        public Task Apply(Order root, OrderItemAddedEvent evt)
        {
            var orderItem = new OrderItem
            {
                Id = evt.OrderItemId,
                Amount = evt.Amount,
                Ordinal = evt.Ordinal,
                Qty = evt.Qty,
                QtyUnit = evt.QtyUnit,
                ProductId = evt.ProductId,
                ProductText = evt.ProductText,
                Order = root
            };

            root.Items.Add(orderItem);
            root.Total = root.Items.Sum(i => i.Amount) - root.Discount;

            return Task.CompletedTask;
        }
    }
}