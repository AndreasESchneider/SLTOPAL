using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;

namespace slClientBus
{
    public interface IClientBusQueue : IDisposable
    {
        event EventHandler<QueueDescription> CreateQueue;

        event EventHandler<SharedAccessAuthorizationRule> CreateSharedAccessRule;

        event EventHandler<BrokeredMessage> MessageReceived;

        List<T> ReceiveBatch<T>(int batch);

        void SendMessage(BrokeredMessage message);

        void SetAutoComplete(bool autoComplete);

        void SetAutoRenewTimeout(TimeSpan autoRenewTimeOut);

        bool Start(string queueName);

        void Stop();
    }
}