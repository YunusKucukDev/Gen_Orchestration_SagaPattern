using MassTransit;
using Order.API.Models.Context;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
    {

        private readonly OrderDbContext _orderDbContext;

        public OrderFailedEventConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }

        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Order.API.Models.Entities.Order order =  await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = Models.Enums.OrderStatu.Fail;
                await _orderDbContext.SaveChangesAsync();
            }
        }
    }
}
