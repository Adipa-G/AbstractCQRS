using System;
using System.Collections.Generic;

namespace AbstractCqrs.Sample.Domain.Order.Model
{
    public class OrderModel
    {
        public OrderModel()
        {
            Items = new List<OrderItemModel>();
        }

        public Guid Id { get; set; }

        public string CustomerNotes { get; set; }

        public string OrderNo { get; set; }

        public double Discount { get; set; }

        public double Total { get; set; }

        public IList<OrderItemModel> Items { get; set; }
    }
}