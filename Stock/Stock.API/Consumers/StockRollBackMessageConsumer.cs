using MassTransit;
using MongoDB.Driver;
using Shared.Events.StockEvents;
using Shared.Messages;
using Shared.Settings;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class StockRollBackMessageConsumer : IConsumer<StockRollBackMessage>
    {

        private readonly MongoDbService _mongoDbService;
        ISendEndpointProvider _sendEndpointProvider;

        public StockRollBackMessageConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider)
        {
            _mongoDbService = mongoDbService;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var stockCollection = _mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await stockCollection.Find(x => x.ProductId == orderItem.ProductId).FirstOrDefaultAsync();

                stock.Count += orderItem.Count;
                await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);

            }
        }
    }
}

