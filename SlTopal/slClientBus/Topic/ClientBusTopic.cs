using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace slClientBus
{
    public class ClientBusTopic : IClientBusTopic
    {
        private readonly string _connectionString;
        private bool _autoComplete;
        private TimeSpan _autoRenewTimeOut;
        private SubscriptionClient _subscriptionClient;
        private Filter _subscriptionFilter;
        private TopicClient _topicClient;

        public ClientBusTopic(string connectionString)
        {
            _connectionString = connectionString;
        }

        public event EventHandler<SharedAccessAuthorizationRule> CreateSharedAccessRule;

        public event EventHandler<TopicDescription> CreateTopic;

        public event EventHandler<BrokeredMessage> MessageReceived;

        public void SendMessage(BrokeredMessage message)
        {
            _topicClient.Send(message);
        }

        public void SetAutoComplete(bool autoComplete)
        {
            _autoComplete = autoComplete;
        }

        public void SetAutoRenewTimeout(TimeSpan autoRenewTimeOut)
        {
            _autoRenewTimeOut = autoRenewTimeOut;
        }

        public void SetSubscriptionFilter(Filter subscriptionFilter)
        {
            _subscriptionFilter = subscriptionFilter;
        }

        public void Start(string topicName, string subscriber)
        {
            Initialise(_connectionString, topicName, subscriber);

            _subscriptionClient.OnMessage(message =>
            {
                try
                {
                    OnMessageReceived(message);
                }
                catch (Exception)
                {
                    message?.Abandon();
                }
            }, new OnMessageOptions
            {
                AutoComplete = _autoComplete,
                AutoRenewTimeout = _autoRenewTimeOut,
                MaxConcurrentCalls = 1
            });
        }

        public void Stop()
        {
            if (_topicClient != null && !_topicClient.IsClosed)
            {
                _topicClient.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void Initialise(string connectionString, string topicName, string subscriber)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (CreateTopic != null)
            {
                if (!namespaceManager.TopicExists(topicName))
                {
                    var newTopicDescription = new TopicDescription(topicName)
                    {
                        DefaultMessageTimeToLive = TimeSpan.MaxValue,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(5),
                        EnableFilteringMessagesBeforePublishing = true
                    };

                    OnCreateTopic(newTopicDescription);

                    namespaceManager.CreateTopic(newTopicDescription);
                }

                if (CreateSharedAccessRule != null)
                {
                    var topicDescription = namespaceManager.GetTopic(topicName);

                    var defaultAccess = new SharedAccessAuthorizationRule(
                        "SharedQueueKey",
                        SharedAccessAuthorizationRule.GenerateRandomKey(),
                        SharedAccessAuthorizationRule.GenerateRandomKey(),
                        new[]
                        {
                            AccessRights.Listen,
                            AccessRights.Send
                        }
                    );

                    OnCreateSharedAccessRule(defaultAccess);

                    SharedAccessAuthorizationRule rule;

                    if (!topicDescription.Authorization.TryGetSharedAccessAuthorizationRule(defaultAccess.KeyName, out rule))
                    {
                        topicDescription.Authorization.Add(defaultAccess);

                        namespaceManager.UpdateTopic(topicDescription);
                    }
                }
            }

            _topicClient = TopicClient.CreateFromConnectionString(connectionString, topicName);

            if (!namespaceManager.SubscriptionExists(topicName, subscriber))
            {
                if (_subscriptionFilter == null)
                {
                    namespaceManager.CreateSubscription(topicName, subscriber);
                }
                else
                {
                    namespaceManager.CreateSubscription(topicName, subscriber, _subscriptionFilter);
                }
            }

            _subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriber, ReceiveMode.PeekLock);
        }

        private void OnCreateSharedAccessRule(SharedAccessAuthorizationRule e)
        {
            CreateSharedAccessRule?.Invoke(this, e);
        }

        private void OnCreateTopic(TopicDescription e)
        {
            CreateTopic?.Invoke(this, e);
        }

        private void OnMessageReceived(BrokeredMessage e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }
}