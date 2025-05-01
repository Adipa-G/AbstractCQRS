using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.View;
using AbstractCqrs.Sample.Service.Product.View;

using Microsoft.EntityFrameworkCore;

namespace AbstractCqrs.Sample.WebApi.Product
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class ViewWriter : IViewWriter
    {
        public async Task UpdateView(IRoot root)
        {
            using (var context = new ProductDbContext())
            {
                var set = context.Set<Domain.Product.Product>();

                var existing =
                    await set.SingleOrDefaultAsync(p => p.Id == root.Id);
                if (existing != null)
                {
                    set.Remove(existing);
                    await context.SaveChangesAsync();
                }

                await context.Set<Domain.Product.Product>()
                    .AddAsync((Domain.Product.Product)root);
                await context.SaveChangesAsync();
            }
        }
    }
}