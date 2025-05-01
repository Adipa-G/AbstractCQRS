using System;

namespace AbstractCqrs.Sample.Domain.Order.Commands
{
    public class UpdateOrderCommand : BaseCommand
    {
        public string CustomerNotes { get; set; }

        public double Discount { get; set; }

        public UpdateOrderCommand(Guid rootId,
            string customerNotes,
            double discount) : base(rootId)
        {
            CustomerNotes = customerNotes;
            Discount = discount;
        }
    }
}