using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Settings;
using StateMachineService;
using StateMachineService.StateDbContext;
using StateMachineService.StateInstance;
using StateMachineService.StateMachines;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddMassTransit(configure =>
{
    configure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(r =>
        {
            r.AddDbContext<DbContext, OrderStateDbContext>((provider, optionsBuilder) =>
            {
                optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("StateMachineSqlServerConnectionString"));
            });
        }); 

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQUrl"));

        cfg.ReceiveEndpoint(RabbitMqSettings.StateMachineQueue, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(context);
        });
    });
});

var host = builder.Build();
host.Run();
