using System;
using System.Threading.Tasks;

using AbstractCqrs.Sample.Domain.Order.Model;

namespace AbstractCqrs.Sample.Domain.Order.View
{
    public interface IOrderView
    {
        Task<OrderModel> GetById(Guid id);
    }
}