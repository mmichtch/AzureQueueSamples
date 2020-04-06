using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Queue;

using System;

namespace HostedQueueProcessor.Core
{

    public class MessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly QueueCollection _queues;

        public MessageProcessor(ILogger<MessageProcessor> logger, QueueCollection queues)
        {
            _logger = logger;
            _queues = queues;
        }

        public void ProcessQueueMessage([QueueTrigger("%QueueCollection:SourceQueue%")] string message)
        {
            _logger.LogInformation($"Processing message: {message}");
            // process message here
            if (message == "error")
            {
                _queues["FailureQueue"].AddMessage(new CloudQueueMessage(message));
            }
            else
            {
                _queues["SuccessQueue"].AddMessage(new CloudQueueMessage(message));
            }
        }
    }

}