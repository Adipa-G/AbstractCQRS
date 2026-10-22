# Abstract CQRS

Abstract CQRS is a implementation of CQRS pattern with event sourcing. The design accommodates the scalability of the application and geared towards micro services architecture. 

Abstract CQRS is designed to target .Net Standard 2.0, the sample projects are .Net Core 2.0

# Quick Start
Quick start documentation describes bare minimum approach to implement and get AbstractCQRS working. Refer to samples to see better ways of implementing it.

## 1. Create Project
create a new project and add AbstractCqrs.Core project as a dependency

## 2. Defining commands and command handlers
Extract from the Sample is used to demonstrate how to define the command and the handler.

```C#
public abstract class BaseCommand : ICommand, IBusMessage
{
     protected BaseCommand(Guid rootId)
     {
          RootId = rootId;
     }

     public Guid RootId { get; }
}

public class CreateOrUpdateProductCommand : BaseCommand
{
     public CreateOrUpdateProductCommand(Guid rootId,
          string code,
          string name,
          double unitPrice) : base(rootId)
     {
          Code = code;
          Name = name;
          UnitPrice = unitPrice;
     }

     public string Code { get; }

     public string Name { get; }

     public double UnitPrice { get; }
}

public class CreateOrUpdateProductCommandHandler : ICommandHandler<Product,CreateOrUpdateProductCommand>
{
     public Task<IList<IEvent<Product>>> Handle(Product root,
          CreateOrUpdateProductCommand cmd)
     {
          IList<IEvent<Product>> list = new List<IEvent<Product>>();
          if (root.Id == Guid.Empty)
          {
               list.Add(new ProductCreatedEvent(cmd.RootId, cmd.Code, cmd.Name,
               cmd.UnitPrice));
          }
          else
          {
               if (root.IsDeleted)
               {
               throw new DomainException($"Product {cmd.RootId} is deleted.");
               }
               
               list.Add(new ProductUpdatedEvent(cmd.RootId, cmd.Code, cmd.Name,
               cmd.UnitPrice));
          }
          return Task.FromResult(list);
     }
}
``` 

Handler is invoked with aggregate root and command as arguments. If the aggregate root is not found in the event store, then a new one is created and passed in.

The result of the command handler is a list of events.

## 3. Defining events and event handlers

Extract from the Sample is used to demonstrate how to define the event and the handler.

```C#
public abstract class BaseEvent<T> : IEvent<T> where T : IRoot
{
        protected BaseEvent(Guid rootId)
        {
                RootId = rootId;
        }

        public Guid RootId { get; }
}

public class ProductCreatedEventHandler : IEventHandler<Product, ProductCreatedEvent>
{
        public async Task Apply(Product root, ProductCreatedEvent evt)
        {
                root.IsDeleted = false;
                root.Code = evt.Code;
                root.Name = evt.Name;
                root.UnitPrice = evt.UnitPrice;
                root.Id = evt.RootId;
                await Task.CompletedTask;
        }
}
```

The event handler has Apply method which accepts aggregate root and an event as parameters. The method will alter the aggregate root based on the information in the event. 

## 4. Implement Unit Of Work

A good example for the unit of work is a database transaction. The unit of work factory provides a mechanism to create a new unit of work.

```C#
public class UnitOfWork : IUnitOfWork
{
     public UnitOfWork(){
          //Create transaction
     }

     public Task Complete()
     {
          //Complete it
     }

     public void Dispose()
     {
     }
}

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
     public IUnitOfWork Create()
     {
          //return a new unit of work
     }
}
```

## 5. Implement Service locator

Service locator is used to find services such as command handlers, event handlers. The sub class of `BaseServiceLocator` and `BaseScope` can be used to implement this. The `BaseServiceLocator` is IOC agnostic, and at this point the IOC container is linked. The given example uses the IServiceProvider and IServiceScope, and it can be replaced with any other IOC.

```C#
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
```

## 6. Implement the Event store

Event store is the heart of the system. 

An interface is provided to define an `EventRecord` and a `BaseEventStore` is provided for easy implementation. Easiest way to achieve this to subclass the `BaseEventStore`

```
public class EventRecord : IEventRecord
{
     public Guid RootId { get; set; }

     public DateTime EventTimestamp { get; set; }

     public string Data { get; set; }
}

public class SqlEventStore : BaseEventStore
{
     protected override IEventRecord Create()
     {
          return new EventRecord();
     }

     protected override string Serialize<TRoot>(IEvent<TRoot> evt)
     {
          var settings =
               new JsonSerializerSettings
               {
               TypeNameHandling =
                    TypeNameHandling.All
               };
          return JsonConvert.SerializeObject(evt, settings);
     }

     protected override IEvent<TRoot> DeSerialize<TRoot>(string data)
     {
          var settings =
               new JsonSerializerSettings
               {
               TypeNameHandling =
                    TypeNameHandling.All
               };
          return JsonConvert.DeserializeObject<IEvent<TRoot>>(data, settings);
     }

     protected override async Task Save(IList<IEventRecord> records)
     {
          //save records to the db
     }

     protected override async Task<IList<IEventRecord>> Load(Guid rootId)
     {
          //load records from db
     }

     public override async Task<Type> GetRootType(Guid rootId)
     {
          //find and return the root type from the persisted events
     }
}
```

## 7. Add the service

Individual micro service is represented in the service. `AbstractCqrs.Core.Services` namespace contains infrastructure to implement a service. Easiest approach is to subclass the `BaseService` class. Each service has a Id and needs to subscribe to a list of buses where service receives commands.

```C#
public interface IProductService : IService
{
}

public class ProductService : BaseService, IProductService
{
        public ProductService(ILog log,
                IServiceLocator serviceLocator) : base(log,
                serviceLocator)
        {
                Id = Constants.ServiceId;
        }

        protected override IList<string> GetSubscriptionBusIds()
        {
            return new List<string> {Constants.BusId};
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
```

## 8. Implement the view writer

View writer is used to update the query stack once a command is processed and events are persisted. For the examples, entity framework is used to persist the AggregateRoot directly into the database. 

```C#
public class ViewWriter : IViewWriter
{
        public async Task UpdateView(IRoot root)
        {
                using (var context = new ProductDbContext())
                {
                var set = context.Set<Domain.Product.Product>();

                var existing =
                        await set.SingleOrDefaultAsync(p => p.Id == root.Id);
                if (existing != null)
                {
                        set.Remove(existing);
                        await context.SaveChangesAsync();
                }

                await context.Set<Domain.Product.Product>()
                        .AddAsync((Domain.Product.Product) root);
                await context.SaveChangesAsync();
                }
        }
}
```

## 9. Putting it all together

Once above steps are completed, now it's time to put it together. The endpoint used in this example is a Web API endpoint. In order to do that the project need `Microsoft.AspNetCore.All` as a reference. 

All services needs to be registered with the selected IOC container. To make this easier, an attribute provided to make registration easier and fault tolerant. `ServiceRegistrationAttribute` attribute can be used to mark services in order to find them using provided class `ResolveUtils`. 

```C#
[ServiceRegistration(ServiceRegistrationType.Singleton)]
public class ProductService : BaseService, IProductService
{
        ...
}
```

In above code the `ProductService` is marked with `ServiceRegistration` attribute.

```C#
public class Startup
{
        public void ConfigureServices(IServiceCollection services)
        {
                ConfigureIOC(services);
                services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
                app.UseMvc();

                var log = app.ApplicationServices.GetService<ILogStart>();
                log.Init();

                var productView =
                app.ApplicationServices.GetService<IProductView>() as
                        ProductDbContext;
                productView.Database.Migrate();

                var locator =
                app.ApplicationServices.GetService<IServiceLocator>() as
                        ServiceLocator;
                locator.Init(app.ApplicationServices);
                locator.Register(new InternalBus(Constants.BusId));

                var productService =
                app.ApplicationServices.GetService<IProductService>();
                locator.Register(productService);
                productService.Init();
        }

        private static void ConfigureIOC(IServiceCollection services)
        {
                var assemblies = new[]{
                     //list of assemblies
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

public class Program
{
        public static void Main(string[] args)
        {
                CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
                return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                        webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel(options =>
                                {
                                options.Listen(IPAddress.Loopback, 5000);
                                });
                });
        }
}
```
Above code is the bootstrap for the application. In the example the `ServiceRegistrationAttribute` to find all services in the list of assemblies and register with the IOC.

Further, the command mappings / command handlers and event handlers are also registered with the IOC container.

# Samples

The projects start with the AbstractCqrs.Sample namespace are sample projects. There are 2 APIs are included in the samples. Further Samples shows One large API (`AbstractCqrs.Sample.WebApi.Combined`) can be split into 2 different small micro services (`AbstractCqrs.Sample.WebApi.Product` and `AbstractCqrs.Sample.WebApi.Order`).

# Performance

When the `AbstractCqrs.Sample.WebApi.Combined` project is executed and `AbstractCqrs.Sample.Performance` is executed, the stack was able to handle 32 commands per second (on a 4th generation Core i5 mobile processor with 8Gb of RAM and a SSD).



