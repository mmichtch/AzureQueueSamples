using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace HostedQueueProcessor.Core
{
    /// Usage depends upon config section named "QueueCollection".  As many queue definitions as needed can be included inside.  Queue keys can
    /// be arbitrarily named, and are used when referencing the QueueCollection indexer.  Example:
    /// 
    /// "QueueCollectionOptions": {
    ///    "CreateIfNotExists" : "false"
    ///    "Queues" : {
    ///       "SourceQueue": "dockets-to-be-validated",
    ///       "SuccessQueue": "dockets-to-be-added-to-b1",
    ///       "FailureQueue": "dockets-where-validation-failed"
    ///    }
    /// }
    public class QueueCollection
    {
        private readonly Dictionary<string, CloudQueue> _queueDict = new Dictionary<string, CloudQueue>();
        private readonly ILogger<QueueCollection> _logger;


        public QueueCollection(ILogger<QueueCollection> logger, CloudQueueClient queueClient, IOptions<QueueCollectionOptions> options)
        {
            _logger = logger;

            if (queueClient == null)
            {
                var userMessage = $"{nameof(CloudQueueClient)} reference passed to {nameof(QueueCollection)} constructor is null.  Ensure service is correctly configured.";
                _logger.LogError(userMessage);
                throw new ArgumentNullException(userMessage);
            }

            if (options == null)
            {
                var userMessage = $"{nameof(QueueCollectionOptions)} is null.  Check configuration.";
                _logger.LogError(userMessage);
                throw new ArgumentNullException(userMessage);
            }

            var opts = options.Value;

            foreach (var queueSetting in opts.Queues)
            {
                var queueKey = queueSetting.Key;
                string queueName = queueSetting.Value;

                _logger.LogDebug($"{nameof(GetQueue)}():  Getting Queue with name '{queueName}' and {nameof(opts.CreateIfNotExists)}={opts.CreateIfNotExists}.");
                var queue = GetQueue(queueClient, queueName, opts.CreateIfNotExists);

                _logger.LogDebug($"{nameof(GetQueue)}():  Storing Queue using key '{queueKey}'.");
                _queueDict.Add(queueKey, queue);
            }
        }


        private CloudQueue GetQueue(CloudQueueClient client, string name, bool createIfNotExists)
        {
            var queue = client.GetQueueReference(name);

            if (createIfNotExists && queue.CreateIfNotExists())
            {
                _logger.LogInformation($"{nameof(GetQueue)}():  Queue '{name}' created.");
            }

            if(!queue.Exists())
            {
                var userMessage = $"{nameof(GetQueue)}():  Queue '{name}' does not exist.  Check your {nameof(QueueCollectionOptions)} configuration.  Consider adding {nameof(QueueCollectionOptions)}:{nameof(QueueCollectionOptions.CreateIfNotExists)}=true setting.";
                _logger.LogError(userMessage);
                throw new InvalidOperationException(userMessage);
            }

            return queue;
        }


        public CloudQueue this[string queueKey]
        {
            get
            {
                if (_queueDict.ContainsKey(queueKey)) return _queueDict[queueKey];

                var userMessage = $"{nameof(QueueCollection)}:indexer:  Attempt to reference a queue with key '{queueKey}' failed.  No such queue key.  Make sure your configuration has the path {nameof(QueueCollectionOptions)}:{nameof(QueueCollectionOptions.Queues)}:{queueKey} with a value corresponding the name of the actual Azure {nameof(CloudQueue)}.";
                _logger.LogError(userMessage);
                throw new InvalidOperationException(userMessage);
            }
        }
    }
}
