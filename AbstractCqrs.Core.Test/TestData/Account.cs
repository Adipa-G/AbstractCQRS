using System;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Core.Test.TestData
{
    public class Account : IRoot
    {
        public double Balance { get; set; }
        public Guid Id { get; set; }
    }
}