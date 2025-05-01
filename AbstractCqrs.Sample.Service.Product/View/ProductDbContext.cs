using System;
using System.Threading.Tasks;

using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Domain.Product.Model;
using AbstractCqrs.Sample.Domain.Product.View;
using AbstractCqrs.Sample.Service.Product.View.Mappings;

using Microsoft.EntityFrameworkCore;

namespace AbstractCqrs.Sample.Service.Product.View
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class ProductDbContext : DbContext, IProductView
    {
        public async Task<ProductModel> GetById(Guid id)
        {
            var product = await Set<Domain.Product.Product>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            return Convert(product);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(Config.ConnectionString);
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            new ProductMapping().Map(
                modelBuilder.Entity<Domain.Product.Product>());
        }

        private ProductModel Convert(Domain.Product.Product product)
        {
            if (product == null)
                return null;

            return new ProductModel
            {
                Id = product.Id,
                UnitPrice = product.UnitPrice,
                Code = product.Code,
                Name = product.Name
            };
        }
    }
}