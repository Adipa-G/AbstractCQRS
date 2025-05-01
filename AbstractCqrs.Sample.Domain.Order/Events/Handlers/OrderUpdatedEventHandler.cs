using System.Linq;
using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Order.Events.Handlers
{
    public class OrderUpdatedEventHandler : IEventHandler<Order, OrderUpdatedEvent>
    {
        public Task Apply(Order root, OrderUpdatedEvent evt)
        {
            root.Id = evt.RootId;
            root.CustomerNotes = evt.CustomerNotes;
            root.Discount = evt.Discount;
            root.Total = root.Items.Sum(i => i.Amount) - root.Discount;

            return Task.CompletedTask;
        }
    }
}