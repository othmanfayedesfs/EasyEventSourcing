﻿using EasyEventSourcing.Domain.Store;
using System;
using System.Collections.Generic;
using EasyEventSourcing.EventSourcing.Exceptions;
using EasyEventSourcing.EventSourcing.Handlers;
using EasyEventSourcing.EventSourcing.Persistence;
using EasyEventSourcing.Messages;
using EasyEventSourcing.Messages.Store;
using EasyEventSourcing.Messages.Orders;
using EasyEventSourcing.Messages.Shipping;

namespace EasyEventSourcing.Application.Write
{
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly Dictionary<Type, Func<IHandler>> handlerFactories = new Dictionary<Type, Func<IHandler>>();

        public CommandHandlerFactory(IEventStore eventStore)
        {
            Func<IRepository> newTransientRepo = () => new Repository(eventStore);

            this.RegisterHandlerFactoryWithTypes(
                () => new ShoppingCartHandler(newTransientRepo()),
                typeof(CreateNewCart), typeof(AddProductToCart), typeof(RemoveProductFromCart), typeof(EmptyCart), typeof(Checkout));

            this.RegisterHandlerFactoryWithTypes(
                () => new OrderHandler(newTransientRepo()),
                typeof(PayForOrder), typeof(ConfirmShippingAddress), typeof(ShipOrder));
        }

        private void RegisterHandlerFactoryWithTypes(Func<IHandler> handler, params Type[] types)
        {
            foreach(var type in types)
            {
                this.handlerFactories.Add(type, handler);
            }
        }

        public ICommandHandler<TCommand> Resolve<TCommand>() where TCommand : ICommand
        {
            if (this.handlerFactories.ContainsKey(typeof(TCommand)))
            {
                var handler = this.handlerFactories[typeof(TCommand)]() as ICommandHandler<TCommand>;
                if (handler != null)
                {
                    return handler;
                }
            }
            throw new NoCommandHandlerRegisteredException(typeof (TCommand));
        }
    }
}