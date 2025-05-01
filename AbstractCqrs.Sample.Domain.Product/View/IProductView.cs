using System;
using System.Threading.Tasks;

using AbstractCqrs.Sample.Domain.Product.Model;

namespace AbstractCqrs.Sample.Domain.Product.View
{
    public interface IProductView
    {
        Task<ProductModel> GetById(Guid id);
    }
}