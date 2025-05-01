using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain.Product.Events.Handlers
{
    public class
        ProductCreatedEventHandler : IEventHandler<Product, ProductCreatedEvent>
    {
        public async Task Apply(Product root, ProductCreatedEvent evt)
        {
            root.IsDeleted = false;
            root.Code = evt.Code;
            root.Name = evt.Name;
            root.UnitPrice = evt.UnitPrice;
            root.Id = evt.RootId;
            await Task.CompletedTask;
        }
    }
}