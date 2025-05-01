using System;

using AbstractCqrs.Core.Command;

namespace AbstractCqrs.Core.Test.TestData
{
    public class UpdateAccountBalanceCommand : ICommand
    {
        public Guid CommandId { get; set; }

        public double Change { get; set; }
        public Guid RootId { get; set; }
    }
}