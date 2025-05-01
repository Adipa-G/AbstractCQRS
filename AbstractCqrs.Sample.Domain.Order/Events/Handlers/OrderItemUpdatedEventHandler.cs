using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Order.Events.Handlers
{
    public class OrderItemUpdatedEventHandler : IEventHandler<Order, OrderItemUpdatedEvent>
    {
        public Task Apply(Order root, OrderItemUpdatedEvent evt)
        {
            var orderItem = root.Items.Single(i => i.Id == evt.OrderItemId);

            orderItem.Amount = evt.Amount;
            orderItem.Qty = evt.Qty;
            orderItem.ProductText = evt.ProductText;
            orderItem.Ordinal = evt.Ordinal;

            root.Total = root.Items.Sum(i => i.Amount) - root.Discount;

            return Task.CompletedTask;
        }
    }
}