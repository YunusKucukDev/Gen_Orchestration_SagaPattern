using MassTransit;
using MongoDB.Driver;
using Shared.Events.OrderEvents;
using Shared.Events.StockEvents;
using Shared.Settings;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly MongoDbService _mongoDbService;
        ISendEndpointProvider _sendEndpointProvider;

        public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider)
        {
            _mongoDbService = mongoDbService;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResults = new();
            var stockCollection = _mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
                stockResults.Add(await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());


            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));


            if (stockResults.TrueForAll(s => s.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    if (stock != null)
                    {
                        stock.Count -= orderItem.Count;

                        await stockCollection.FindOneAndReplaceAsync(s => s.ProductId == stock.ProductId, stock);

                    }
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };
                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                    Message = "Stock yetersiz"
                };
                await sendEndpoint.Send(stockNotReservedEvent);

            }
        }
    }
}
