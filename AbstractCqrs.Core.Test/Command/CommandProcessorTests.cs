using System;
using System.Collections.Generic;

using AbstractCqrs.Core.Command;
using AbstractCqrs.Core.Command.Impl;
using AbstractCqrs.Core.Domain;
using AbstractCqrs.Core.EventStore;
using AbstractCqrs.Core.Exceptions;
using AbstractCqrs.Core.Resolve;
using AbstractCqrs.Core.Services;
using AbstractCqrs.Core.Test.TestData;
using AbstractCqrs.Core.View;

using NSubstitute;

using NUnit.Framework;

namespace AbstractCqrs.Core.Test.Command
{
    [TestFixture]
    public class CommandProcessorTests
    {
        private IServiceLocator serviceLocator;
        private IRootTypeResolver rootTypeResolver;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IEventStore eventStore;
        private IViewWriter viewWriter;
        private ILog log;

        private IUnitOfWork unitOfWork;
        private IScope scope;

        [SetUp]
        public void Init()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            rootTypeResolver = Substitute.For<IRootTypeResolver>();
            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            eventStore = Substitute.For<IEventStore>();
            viewWriter = Substitute.For<IViewWriter>();
            log = Substitute.For<ILog>();

            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWorkFactory.Create().Returns(unitOfWork);

            scope = Substitute.For<IScope>();
            serviceLocator.CreateScope().Returns(scope);


            rootTypeResolver.GetRootType(Arg.Any<CreateAccountCommand>())
                .Returns(typeof(Account));

            scope
                .ResolveCommandHandler<Account, CreateAccountCommand>()
                .Returns(new CreateAccountCommandHandler());

            scope
                .ResolveCommandHandler<Account, UpdateAccountBalanceCommand>()
                .Returns(new UpdateAccountBalanceCommandHandler());

            scope.ResolveEventHandler<Account, AccountCreatedEvent>()
                .Returns(new AccountCreatedEventHandler());

            scope
                .ResolveEventHandler<Account, AccountBalanceUpdatedEvent>()
                .Returns(new AccountBalanceUpdatedEventHandler());
        }

        [Test]
        public void GivenCommand_WhenProcess_ThenGenerateEvents()
        {
            var rootId = Guid.NewGuid();

            var cmd = new UpdateAccountBalanceCommand
            {
                RootId = rootId,
                Change = 10
            };

            eventStore.GetRootType(rootId).Returns(typeof(Account));

            eventStore.GetEvents<Account>(rootId)
                .Returns(new List<IEvent<Account>>
                         {
                             new AccountCreatedEvent {RootId = rootId}
                         });


            var sut = CreateSut();
            sut.Process(cmd);

            scope.Received(1)
                .ResolveCommandHandler<Account, UpdateAccountBalanceCommand>();
            scope.Received(1)
                .ResolveEventHandler<Account, AccountCreatedEvent>();
            scope.Received(1)
                .ResolveEventHandler<Account, AccountBalanceUpdatedEvent>();

            unitOfWorkFactory.Received(1).Create();
            unitOfWork.Received(1).Complete();

            eventStore.Received(1).GetEvents<Account>(rootId);
            eventStore.Received(1)
                .SaveEvents(Arg.Any<IList<IEvent<Account>>>());
            viewWriter.Received(1).UpdateView(Arg.Any<Account>());
            log.Received(1).LogDebug(Arg.Any<string>());
        }

        [Test]
        public void GivenCommand_WhenProcess_ThenLoadRoot()
        {
            var rootId = Guid.NewGuid();
            Account account = null;

            var events = new List<IEvent<Account>>();
            events.Add(new AccountCreatedEvent { RootId = rootId });
            events.Add(
                new AccountBalanceUpdatedEvent
                {
                    RootId = rootId,
                    Change = 10
                });

            eventStore.GetRootType(rootId).Returns(typeof(Account));

            eventStore.GetEvents<Account>(rootId).Returns(events);

            viewWriter.When(r => r.UpdateView(Arg.Any<Account>()))
                .Do(r => account = r.Arg<Account>());

            var cmd = new UpdateAccountBalanceCommand
            {
                RootId = rootId,
                Change = 5
            };

            var sut = CreateSut();
            sut.Process(cmd);

            scope.Received(1)
                .ResolveEventHandler<Account, AccountCreatedEvent>();
            scope.Received(2)
                .ResolveEventHandler<Account, AccountBalanceUpdatedEvent>();

            Assert.IsNotNull(account);
            Assert.AreEqual(rootId, account.Id);
            Assert.AreEqual(15, account.Balance);
        }

        [Test]
        public void GivenNoCommandHandlerRegistered_WhenProcess_ThenThrowException()
        {
            scope
                .ResolveCommandHandler<Account, CreateAccountCommand>()
                .Returns((ICommandHandler<Account, CreateAccountCommand>)null);

            var sut = CreateSut();

            Assert.That(() => sut.Process(new CreateAccountCommand()),
                Throws.TypeOf<ServiceLocationException>());
        }

        private ICommandProcessor CreateSut()
        {
            return new CommandProcessor(serviceLocator,
                rootTypeResolver,
                unitOfWorkFactory,
                eventStore,
                viewWriter,
                log);
        }
    }
}