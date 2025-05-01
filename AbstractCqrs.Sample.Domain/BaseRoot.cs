using System;

using AbstractCqrs.Core.Domain;

namespace AbstractCqrs.Sample.Domain
{
    public abstract class BaseRoot : IRoot
    {
        public bool IsDeleted { get; set; }
        public Guid Id { get; set; }
    }
}