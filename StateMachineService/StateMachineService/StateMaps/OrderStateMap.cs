using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StateMachineService.StateInstance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineService.StateMaps
{
    public class OrderStateMap : SagaClassMap<OrderStateInstance>
    {
        override protected void Configure(EntityTypeBuilder<OrderStateInstance> entity,ModelBuilder model)
        {
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.BuyerId).IsRequired();
            entity.Property(x => x.TotalPrice).HasDefaultValue(0);
        }
    }
}
