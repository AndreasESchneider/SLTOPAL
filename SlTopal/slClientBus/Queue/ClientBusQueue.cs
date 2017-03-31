using System;
using System.Collections.Generic;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace slClientBus
{
    public class ClientBusQueue : IClientBusQueue
    {
        private readonly string _connectionString;
        private bool _autoComplete;
        private TimeSpan _autoRenewTimeOut;
        private QueueClient _queueClient;

        public ClientBusQueue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public event EventHandler<QueueDescription> CreateQueue;

        public event EventHandler<SharedAccessAuthorizationRule> CreateSharedAccessRule;

        public event EventHandler<BrokeredMessage> MessageReceived;

        public List<T> ReceiveBatch<T>(int batch)
        {
            var result = new List<T>();

            var messages = _queueClient.ReceiveBatch(batch, TimeSpan.FromSeconds(30));

            foreach (var message in messages)
            {
                if (!message.ContentType.Equals(typeof(T).Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(message.GetBody<T>());

                _queueClient.Complete(message.LockToken);
            }

            return result;
        }

        public void SendMessage(BrokeredMessage message)
        {
            _queueClient.Send(message);
        }

        public void SetAutoComplete(bool autoComplete)
        {
            _autoComplete = autoComplete;
        }

        public void SetAutoRenewTimeout(TimeSpan autoRenewTimeOut)
        {
            _autoRenewTimeOut = autoRenewTimeOut;
        }

        public bool Start(string queueName)
        {
            if (!Initialize(_connectionString, queueName))
            {
                return false;
            }

            if (MessageReceived != null)
            {
                _queueClient.OnMessage(message =>
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

            return true;
        }

        public void Stop()
        {
            if (_queueClient == null || _queueClient.IsClosed)
            {
                return;
            }

            CreateQueue = null;
            CreateSharedAccessRule = null;
            MessageReceived = null;

            _queueClient.Close();
        }

        public void Dispose()
        {
            Stop();
        }

        public List<T> Peek<T>(int batch)
        {
            var result = new List<T>();

            var messages = _queueClient.PeekBatch(batch);

            foreach (var message in messages)
            {
                if (!message.ContentType.Equals(typeof(T).Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(message.GetBody<T>());

                _queueClient.Abandon(message.LockToken);
            }

            return result;
        }

        private bool Initialize(string connectionString, string queueName)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (CreateQueue != null)
            {
                if (!namespaceManager.QueueExists(queueName))
                {
                    var newQueueDescription = new QueueDescription(queueName)
                    {
                        MaxDeliveryCount = int.MaxValue,
                        DefaultMessageTimeToLive = TimeSpan.MaxValue,
                        EnableDeadLetteringOnMessageExpiration = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(5),
                        LockDuration = TimeSpan.FromMinutes(3),
                        RequiresSession = false
                    };

                    OnCreateQueue(newQueueDescription);

                    namespaceManager.CreateQueue(newQueueDescription);
                }

                if (CreateSharedAccessRule != null)
                {
                    var queueDescription = namespaceManager.GetQueue(queueName);

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

                    if (!queueDescription.Authorization.TryGetSharedAccessAuthorizationRule(defaultAccess.KeyName, out rule))
                    {
                        queueDescription.Authorization.Add(defaultAccess);

                        namespaceManager.UpdateQueue(queueDescription);
                    }
                }
            }

            _queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName, ReceiveMode.PeekLock);

            return _queueClient != null;
        }

        private void OnCreateQueue(QueueDescription e)
        {
            CreateQueue?.Invoke(this, e);
        }

        private void OnCreateSharedAccessRule(SharedAccessAuthorizationRule e)
        {
            CreateSharedAccessRule?.Invoke(this, e);
        }

        private void OnMessageReceived(BrokeredMessage e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }
}