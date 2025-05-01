using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Order.Events.Handlers
{
    public class OrderCreatedEventHandler : IEventHandler<Order, OrderCreatedEvent>
    {
        public Task Apply(Order root, OrderCreatedEvent evt)
        {
            root.Id = evt.RootId;
            root.OrderNo = evt.OrderNo;
            root.CustomerNotes = evt.CustomerNotes;
            root.Discount = evt.Discount;
            root.Total = root.Items.Sum(i => i.Amount) - root.Discount;

            return Task.CompletedTask;
        }
    }
}