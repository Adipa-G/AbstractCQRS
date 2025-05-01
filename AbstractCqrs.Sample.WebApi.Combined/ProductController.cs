using System;
using System.Threading.Tasks;

using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Domain.Product.Commands;
using AbstractCqrs.Sample.Domain.Product.Model;
using AbstractCqrs.Sample.Domain.Product.View;
using AbstractCqrs.Sample.Service.Product;

using Microsoft.AspNetCore.Mvc;

namespace AbstractCqrs.Sample.WebApi.Combined
{
    [Route("api/product")]
    public class ProductController
    {
        private readonly IProductView productView;
        private readonly IServiceLocator serviceLocator;

        public ProductController(IServiceLocator serviceLocator,
            IProductView productView)
        {
            this.serviceLocator = serviceLocator;
            this.productView = productView;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await productView.GetById(id);
            return new JsonResult(product);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] ProductModel model)
        {
            model.Id = Guid.NewGuid();

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new CreateOrUpdateProductCommand(model.Id,
                model.Code, model.Name, model.UnitPrice));

            return new JsonResult(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,
            [FromBody] ProductModel model)
        {
            model.Id = id;

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new CreateOrUpdateProductCommand(id,
                model.Code, model.Name, model.UnitPrice));

            return new JsonResult(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new DeleteProductCommand(id));

            return new OkResult();
        }
    }
}