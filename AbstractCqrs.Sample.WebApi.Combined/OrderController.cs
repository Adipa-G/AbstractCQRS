using System;
using System.Threading.Tasks;

using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Domain.Order.Commands;
using AbstractCqrs.Sample.Domain.Order.Model;
using AbstractCqrs.Sample.Domain.Order.View;
using AbstractCqrs.Sample.Service.Order;

using Microsoft.AspNetCore.Mvc;

namespace AbstractCqrs.Sample.WebApi.Combined
{
    [Route("api/order")]
    public class OrderController
    {
        private readonly IOrderView orderView;
        private readonly IServiceLocator serviceLocator;

        public OrderController(IServiceLocator serviceLocator,
            IOrderView orderView)
        {
            this.serviceLocator = serviceLocator;
            this.orderView = orderView;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await orderView.GetById(id);
            return new JsonResult(order);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] OrderModel model)
        {
            model.Id = Guid.NewGuid();

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new CreateOrderCommand(model.Id, model.CustomerNotes,
                model.OrderNo, model.Discount));

            return new JsonResult(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,
            [FromBody] OrderModel model)
        {
            model.Id = id;

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new UpdateOrderCommand(id,
                model.CustomerNotes, model.Discount));

            return new JsonResult(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new DeleteOrderCommand(id));

            return new OkResult();
        }

        [HttpPost("item/{id}")]
        public async Task<IActionResult> AddItem(Guid id, [FromBody] OrderItemModel model)
        {
            model.Id = Guid.NewGuid();

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new AddOrderItemCommand(id, model.Id, model.Ordinal,
                model.ProductId, model.ProductText, model.Qty, model.QtyUnit,
                model.Amount));

            return new JsonResult(model);
        }

        [HttpPut("item/{id}/{itemId}")]
        public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] OrderItemModel model)
        {
            model.Id = itemId;

            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new UpdateOrderItemCommand(id, itemId, model.Ordinal,
                model.ProductText, model.Qty, model.Amount));

            return new JsonResult(model);
        }

        [HttpDelete("item/{id}/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
        {
            var bus = serviceLocator.ResolveBus(Constants.BusId);
            await bus.Send(new RemoveOrderItemCommand(id, itemId));

            return new OkResult();
        }
    }
}