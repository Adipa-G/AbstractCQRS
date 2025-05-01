using System.Threading.Tasks;

using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.View;
using AbstractCqrs.Sample.Domain.Order;
using AbstractCqrs.Sample.Domain.Product;
using AbstractCqrs.Sample.Service.Order.View;
using AbstractCqrs.Sample.Service.Product.View;

using Microsoft.EntityFrameworkCore;

namespace AbstractCqrs.Sample.WebApi.Combined
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class ViewWriter : IViewWriter
    {
        public async Task UpdateView(IRoot root)
        {
            await IfOrder(root);
            await IfProduct(root);
        }

        private static async Task IfOrder(IRoot root)
        {
            var order = root as Order;
            if (order != null)
            {
                using (var context = new OrderDbContext())
                {
                    var set = context.Set<Order>();

                    var existing =
                        await set.SingleOrDefaultAsync(p => p.Id == order.Id);
                    if (existing != null)
                    {
                        set.Remove(existing);
                        await context.SaveChangesAsync();
                    }

                    await context.Set<Order>()
                        .AddAsync(order);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task IfProduct(IRoot root)
        {
            var product = root as Product;
            if (product != null)
            {
                using (var context = new ProductDbContext())
                {
                    var set = context.Set<Product>();

                    var existing =
                        await set.SingleOrDefaultAsync(p => p.Id == root.Id);
                    if (existing != null)
                    {
                        set.Remove(existing);
                        await context.SaveChangesAsync();
                    }

                    await context.Set<Product>()
                        .AddAsync((Product)root);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}