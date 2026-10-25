using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<Payment.API.Consumers.PaymentStartedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint(Shared.Settings.RabbitMqSettings.Payment_PaymentStartedEventQueue, e =>
            e.ConfigureConsumer<Payment.API.Consumers.PaymentStartedEventConsumer>(context));
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQUrl"));

    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
