using MassTransit;
using Order.API.Models.Context;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        private readonly OrderDbContext _orderDbContext;

        public OrderCompletedEventConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Order.API.Models.Entities.Order order= await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = Models.Enums.OrderStatu.Complete;
                await _orderDbContext.SaveChangesAsync();
            }
        }
    }
}
