using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Settings
{
    public static class RabbitMqSettings
    {
        public const string StateMachineQueue = $"state-machine-queue";
        public const string Stock_OrderCreatedEventQueue = $"stock-order-created-queue";
        public const string Order_OrderCompletedEventQueue = $"order-order-completed-queue";
        public const string Order_OrderFailedEventQueue = $"order-order-failed-queue";
        public const string Stock_RollbackMessageQueue = $"stock-rollback-message-queue";
        public const string Payment_PaymentStartedEventQueue = $"payment-payment-started-queue";

    }
}
