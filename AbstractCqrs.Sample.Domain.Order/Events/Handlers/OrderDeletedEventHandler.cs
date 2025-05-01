using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Order.Events.Handlers
{
    public class OrderDeletedEventHandler : IEventHandler<Order, OrderDeletedEvent>
    {
        public Task Apply(Order root, OrderDeletedEvent evt)
        {
            root.IsDeleted = true;

            return Task.CompletedTask;
        }
    }
}