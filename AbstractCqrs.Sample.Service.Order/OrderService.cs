using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Services;
using AbstractCqrs.Core.Services.Impl;

namespace AbstractCqrs.Sample.Service.Order
{
    [ServiceRegistration(ServiceRegistrationType.Singleton)]
    public class OrderService : BaseService, IOrderService
    {
        public OrderService(ILog log,
            IServiceLocator serviceLocator) : base(log,
            serviceLocator)
        {
            Id = Constants.ServiceId;
        }

        protected override IList<string> GetSubscriptionBusIds()
        {
            return new List<string> { Constants.BusId };
        }

        protected override Task OnCommandProcessed(ICommand cmd)
        {
            return Task.FromResult(0);
        }

        protected override Task OnCommandError(ICommand cmd)
        {
            throw new Exception("Unknown error");
        }
    }
}