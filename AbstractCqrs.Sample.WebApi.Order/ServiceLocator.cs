using System;

using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Service.Resolve;

using Microsoft.Extensions.DependencyInjection;

namespace AbstractCqrs.Sample.WebApi.Order
{
    [ServiceRegistration(ServiceRegistrationType.Singleton)]
    public class ServiceLocator : BaseServiceLocator
    {
        private IServiceProvider serviceProvider;

        public void Init(IServiceProvider sp)
        {
            serviceProvider = sp;
        }

        public override IScope CreateScope()
        {
            return new Scope(this, serviceProvider.CreateScope());
        }
    }
}