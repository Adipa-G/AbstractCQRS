﻿using System;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Sample.Service.Resolve;
using Microsoft.Extensions.DependencyInjection;

namespace AbstractCqrs.Sample.WebApi.Product
{
    public class Scope : BaseScope
    {
        private IServiceScope serviceScope;

        public Scope(IServiceLocator serviceLocator, IServiceScope serviceScope) : base(serviceLocator)
        {
            this.serviceScope = serviceScope;
        }

        protected override object Resolve(Type @interface)
        {
            return serviceScope.ServiceProvider.GetService(@interface);
        }

        public override void Dispose()
        {
            serviceScope.Dispose();
        }
    }
}
