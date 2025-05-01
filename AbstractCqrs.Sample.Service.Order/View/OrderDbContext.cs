using System;
using System.Threading.Tasks;

using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Domain.Order.Model;
using AbstractCqrs.Sample.Domain.Order.View;
using AbstractCqrs.Sample.Service.Order.View.Mappings;

using Microsoft.EntityFrameworkCore;

namespace AbstractCqrs.Sample.Service.Order.View
{
    [ServiceRegistration(ServiceRegistrationType.Scope)]
    public class OrderDbContext : DbContext, IOrderView
    {
        public async Task<OrderModel> GetById(Guid id)
        {
            var order = await Set<Domain.Order.Order>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            return Convert(order);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(Config.ConnectionString);
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            new OrderMapping().Map(
                modelBuilder.Entity<Domain.Order.Order>());
            new OrderItemMapping().Map(
                modelBuilder.Entity<Domain.Order.OrderItem>());
        }

        private OrderModel Convert(Domain.Order.Order order)
        {
            if (order == null)
                return null;

            var model = new OrderModel()
            {
                Id = order.Id,
                CustomerNotes = order.CustomerNotes,
                Discount = order.Discount,
                OrderNo = order.OrderNo,
                Total = order.Total
            };

            foreach (var orderItem in order.Items)
            {
                model.Items.Add(new OrderItemModel()
                {
                    Id = orderItem.Id,
                    Amount = orderItem.Amount,
                    Ordinal = orderItem.Ordinal,
                    ProductText = orderItem.ProductText,
                    Qty = orderItem.Qty,
                    ProductId = orderItem.ProductId,
                    QtyUnit = orderItem.QtyUnit
                });
            }

            return model;
        }
    }
}