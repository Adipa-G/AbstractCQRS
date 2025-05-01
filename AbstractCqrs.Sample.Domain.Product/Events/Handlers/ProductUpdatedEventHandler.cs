using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Product.Events.Handlers
{
    public class
        ProductUpdatedEventHandler : IEventHandler<Product, ProductUpdatedEvent>
    {
        public async Task Apply(Product root, ProductUpdatedEvent evt)
        {
            root.Code = evt.Code;
            root.Name = evt.Name;
            root.UnitPrice = evt.UnitPrice;
            await Task.CompletedTask;
        }
    }
}