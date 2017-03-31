using System;
using Microsoft.ServiceBus.Messaging;

namespace slClientBus
{
    public interface IClientBusTopic : IDisposable
    {
        event EventHandler<SharedAccessAuthorizationRule> CreateSharedAccessRule;

        event EventHandler<TopicDescription> CreateTopic;

        event EventHandler<BrokeredMessage> MessageReceived;

        void SendMessage(BrokeredMessage message);

        void SetAutoComplete(bool autoComplete);

        void SetAutoRenewTimeout(TimeSpan autoRenewTimeOut);

        void SetSubscriptionFilter(Filter subscriptionFilter);

        void Start(string topicName, string subscriber);

        void Stop();
    }
}