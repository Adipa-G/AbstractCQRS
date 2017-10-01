﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistCqrs.Core.Command;
using DistCqrs.Core.Exceptions;
using DistCqrs.Core.Resolve;

namespace DistCqrs.Core.Services.Impl
{
    public abstract class BaseService : IService, IBusSubscriber
    {
        private readonly ILog log;
        private readonly IServiceLocator serviceLocator;
       
        public string Id { get; protected set; }

        protected abstract IList<string> GetSubscriptionBusIds();

        protected abstract Task OnCommandProcessed(ICommand cmd);

        protected abstract Task OnCommandError(ICommand cmd);

        protected BaseService(ILog log,
            IServiceLocator serviceLocator)
        {
            this.log = log;
            this.serviceLocator = serviceLocator;
        }

        public void Init()
        {
            foreach (var busId in GetSubscriptionBusIds())
            {
                var bus = serviceLocator.ResolveBus(busId);
                if (bus == null)
                {
                    throw new ServiceLocationException(
                        $"Unable to resolve bus {busId}");
                }
                bus.Subscribe(this);
            }
        }

        public async Task Receive(IBus srcBus, IBusMessage message)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var cmd = message as ICommand;
            if (cmd == null)
            {
                log.LogError(
                    $"Error while processing message {message} in service {Id}");
            }

            try
            {
                var commandProcessor =
                    serviceLocator.Resolve<ICommandProcessor>();

                await commandProcessor.Process(cmd);
                await OnCommandProcessed(cmd);
            }
            catch (Exception ex)
            {
                log.LogException(
                    $"Error while processing command {cmd} in service {Id}",
                    ex);
                await OnCommandError(cmd);
            }
        }

        protected async Task SendMessage(string busId, IBusMessage message)
        {
            var bus = serviceLocator.ResolveBus(busId);
            if (bus == null)
            {
                throw new ServiceLocationException(
                    $"Unable to resolve bus {busId}");
            }
            await bus.Send(message);
        }
    }
}