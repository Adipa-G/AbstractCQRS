using System;

namespace AbstractCqrs.Sample.Domain.Order.Commands
{
    public class CreateOrderCommand : BaseCommand
    {
        public string CustomerNotes { get; set; }

        public string OrderNo { get; set; }

        public double Discount { get; set; }

        public CreateOrderCommand(Guid rootId,
            string customerNotes,
            string orderNo,
            double discount) : base(rootId)
        {
            CustomerNotes = customerNotes;
            OrderNo = orderNo;
            Discount = discount;
        }
    }
}