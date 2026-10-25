using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models.Context;
using Order.API.ViewModals;
using Shared.Events.OrderEvents;
using Shared.Messages;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCompletedEventConsumer>();
    configure.AddConsumer<OrderFailedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(RabbitMqSettings.Order_OrderCompletedEventQueue, e =>
            e.ConfigureConsumer<OrderCompletedEventConsumer>(context));
        
        cfg.ReceiveEndpoint(RabbitMqSettings.Order_OrderFailedEventQueue, e =>
            e.ConfigureConsumer<OrderFailedEventConsumer>(context));

        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQUrl"));
    });
});

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OrderDbConnection")));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order.API v1");
    c.RoutePrefix = "swagger"; 
});

app.UseHttpsRedirection();



app.MapPost("/create-order", async (OrderDbContext dbContext, CreateOrderVM model,ISendEndpointProvider sendEndpointProvider) =>
{
    Order.API.Models.Entities.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = Order.API.Models.Enums.OrderStatu.Suspend,
        TotalPrice = model.OrderItems.Sum(x => x.Count * x.Price),
        OrderItems = model.OrderItems.Select(x => new Order.API.Models.Entities.OrderItem
        {
            ProductId = x.ProductId,
            Count = x.Count,
            Price = x.Price
        }).ToList()
    };

    await dbContext.Orders.AddAsync(order);
    await dbContext.SaveChangesAsync();

    OrderStarterEvent orderStarterEvent= new()
    {
        OrderId = order.Id,
        BuyerId = order.BuyerId,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(x => new OrderItemMessage
        {
            ProductId = x.ProductId,
            Count = x.Count,
            Price = x.Price
        }).ToList()
    };

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));
    await sendEndpoint.Send<OrderStarterEvent>(orderStarterEvent);

    return Results.Ok(order);
});

app.Run();