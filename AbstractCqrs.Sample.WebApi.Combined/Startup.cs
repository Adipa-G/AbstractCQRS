using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Command.Impl;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.Exceptions;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Resolve.Helpers;
using AbstractCqrs.Core.Services.Impl;
using AbstractCqrs.Sample.Domain.Order.View;
using AbstractCqrs.Sample.Domain.Product.View;
using AbstractCqrs.Sample.Service;
using AbstractCqrs.Sample.Service.Log;
using AbstractCqrs.Sample.Service.Order;
using AbstractCqrs.Sample.Service.Order.View;
using AbstractCqrs.Sample.Service.Product;
using AbstractCqrs.Sample.Service.Product.View;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AbstractCqrs.Sample.WebApi.Combined
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureIOC(services);

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Combined API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Combined API");
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var locator =
                app.ApplicationServices.GetService<IServiceLocator>() as
                    ServiceLocator;
            locator.Init(app.ApplicationServices);
            locator.Register(new InternalBus(Service.Order.Constants.BusId));
            locator.Register(new InternalBus(Service.Product.Constants.BusId));

            using (var scope = locator.CreateScope())
            {
                var log = scope.Resolve<ILogStart>();
                log.Init();

                var orderView = scope.Resolve<IOrderView>() as OrderDbContext;
                orderView.Database.Migrate();

                var productView = scope.Resolve<IProductView>() as ProductDbContext;
                productView.Database.Migrate();

                var orderService = scope.Resolve<IOrderService>();
                orderService.Init();

                var productService = scope.Resolve<IProductService>();
                productService.Init();
            }
        }

        private static void ConfigureIOC(IServiceCollection services)
        {
            var assemblies = new[]
                             {
                                 typeof(CommandProcessor).Assembly,
                                 typeof(Domain.Order.Order).Assembly,
                                 typeof(Domain.Product.Product).Assembly,
                                 typeof(Startup).Assembly,
                                 typeof(OrderService).Assembly,
                                 typeof(ProductService).Assembly,
                                 typeof(Config).Assembly
                             };

            var allRegs = ResolveUtils.GetDependencies(assemblies);
            foreach (var reg in allRegs)
                switch (reg.RegistrationType)
                {
                    case ServiceRegistrationType.Scope:
                        services.AddScoped(reg.Interface, reg.Implementation);
                        break;
                    case ServiceRegistrationType.Singleton:
                        services.AddSingleton(reg.Interface,
                            reg.Implementation);
                        break;
                    default:
                        throw new ServiceRegistrationException(
                            $"Unknown registration type {reg.RegistrationType}");
                }

            var commandHandlerMappings =
                ResolveUtils.GetCommandHandlerMappings(assemblies);
            foreach (var mapping in commandHandlerMappings)
            {
                var cmdHanderInterface =
                    typeof(ICommandHandler<,>).MakeGenericType(
                        mapping.EntityType, mapping.CommandType);
                services.AddScoped(cmdHanderInterface,
                    mapping.CommandHandlerType);
            }

            var eventHandlerMappings =
                ResolveUtils.GetEventHandlerMappings(assemblies);
            foreach (var mapping in eventHandlerMappings)
            {
                var eventHandlerInterface =
                    typeof(IEventHandler<,>).MakeGenericType(
                        mapping.EntityType, mapping.EventType);
                services.AddScoped(eventHandlerInterface,
                    mapping.EventHandlerType);
            }
        }
    }
}