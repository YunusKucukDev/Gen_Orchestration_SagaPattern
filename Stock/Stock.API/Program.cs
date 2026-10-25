using MassTransit;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>();
    configure.AddConsumer<Stock.API.Consumers.StockRollBackMessageConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(Shared.Settings.RabbitMqSettings.Stock_OrderCreatedEventQueue, e =>
            e.ConfigureConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>(context));

        cfg.ReceiveEndpoint(Shared.Settings.RabbitMqSettings.Stock_RollbackMessageQueue, e =>
            e.ConfigureConsumer<Stock.API.Consumers.StockRollBackMessageConsumer>(context));

        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQUrl"));

    });
});

builder.Services.AddSingleton<MongoDbService>();


using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

var collection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();

if (collection.EstimatedDocumentCount() == 0)
{
    mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>().InsertOne(new()
    {
        ProductId = 1,
        Count = 100
    });
    mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>().InsertOne(new()
    {
        ProductId = 2,
        Count = 300
    });
    mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>().InsertOne(new()
    {
        ProductId = 3,
        Count = 50
    });
}


var app = builder.Build();
app.MapOpenApi();
app.Run();
