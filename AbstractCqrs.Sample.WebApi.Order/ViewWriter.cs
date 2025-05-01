using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.View;
using AbstractCqrs.Sample.Service.Order.View;

using Microsoft.EntityFrameworkCore;

namespace AbstractCqrs.Sample.WebApi.Order
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class ViewWriter : IViewWriter
    {
        public async Task UpdateView(IRoot root)
        {
            using (var context = new OrderDbContext())
            {
                var set = context.Set<Domain.Order.Order>();

                var existing =
                    await set.SingleOrDefaultAsync(p => p.Id == root.Id);
                if (existing != null)
                {
                    set.Remove(existing);
                    await context.SaveChangesAsync();
                }

                await context.Set<Domain.Order.Order>()
                    .AddAsync((Domain.Order.Order)root);
                await context.SaveChangesAsync();
            }
        }
    }
}