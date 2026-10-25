using MassTransit;
using Shared.Events.OrderEvents;
using Shared.Events.PaymentEvents;
using Shared.Events.StockEvents;
using Shared.Messages;
using Shared.Settings;
using StateMachineService.StateInstance;

namespace StateMachineService.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<OrderStarterEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State Paymentcompleted { get; set; }
        public State PaymentFailed { get; set; }


        public OrderStateMachine()
        {
            InstanceState(inst => inst.CurrentState);

            Event(() => OrderStartedEvent,
                orderStateInstance => orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid())
                );
            Event(() => StockReservedEvent,
                stockStateInstance => stockStateInstance.CorrelateById(@event => @event.Message.CorrelationId)
                );
            Event(() => StockNotReservedEvent,
                stockStateInstance => stockStateInstance.CorrelateById(@event => @event.Message.CorrelationId)
                );
            Event(() => PaymentCompletedEvent,
                paymentStateInstance => paymentStateInstance.CorrelateById(@event => @event.Message.CorrelationId)
                );
            Event(() => PaymentFailedEvent,
                paymentStateInstance => paymentStateInstance.CorrelateById(@event => @event.Message.CorrelationId)
                );


            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.TotalPrice = context.Message.TotalPrice;
                    context.Saga.CreatedDate = DateTime.UtcNow;
                }).TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEventQueue}"),
                    context => new OrderCreatedEvent(context.Saga.CorrelationId)
                    {
                        OrderItems = context.Message.OrderItems
                    }
                ));

            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved).
                Send(new Uri($"queue:{RabbitMqSettings.Payment_PaymentStartedEventQueue}"),
                    context => new PaymentStartedEvent(context.Saga.CorrelationId)
                    {
                        OrderItems = context.Message.OrderItems,
                        Price = context.Saga.TotalPrice
                    }
                ),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent()
                    {
                        OrderId = context.Saga.OrderId,
                        Message = "Stock yetersiz işlem başarısız."

                    }
                ));

            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(Paymentcompleted)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderCompletedEventQueue}"),
                    context => new OrderCompletedEvent()
                    {
                        OrderId = context.Saga.OrderId,
                    }
                )
                .Finalize(),
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent()
                    {
                        OrderId = context.Saga.OrderId,
                        Message = "Ödeme başarısız işlem başarısız."
                    }
                )
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_RollbackMessageQueue}"),
                    context => new StockRollBackMessage()
                    {
                        OrderItems = context.Message.OrderItems
                    }
                ));
            SetCompletedWhenFinalized();
        }

    }
}
