using System.Collections.Generic;

namespace AbstractCqrs.Sample.Domain.Order
{
    public class Order : BaseRoot
    {
        public Order()
        {
            Items = new List<OrderItem>();
        }

        public string CustomerNotes { get; set; }

        public string OrderNo { get; set; }

        public double Discount { get; set; }

        public double Total { get; set; }

        public IList<OrderItem> Items { get; set; }
    }
}