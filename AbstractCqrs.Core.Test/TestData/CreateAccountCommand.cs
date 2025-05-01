using System;

using AbstractCqrs.Core.Command;

namespace AbstractCqrs.Core.Test.TestData
{
    public class CreateAccountCommand : ICommand
    {
        public Guid CommandId { get; set; }
        public Guid RootId { get; set; }
    }
}