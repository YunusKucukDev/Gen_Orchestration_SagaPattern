using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.PaymentEvents
{
    public class PaymentStartedEvent : CorrelatedBy<Guid>
    {
        public PaymentStartedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; set; }
        public decimal Price { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
