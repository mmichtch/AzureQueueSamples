using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostedQueueProcessor.Core
{

    public class MessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;

        public MessageProcessor(ILogger<MessageProcessor> logger)
        {
            _logger = logger;
        }

        public void ProcessQueueMessage([QueueTrigger("%EventQueue%")] string message)
        {
            _logger.LogInformation($"Processing message: {message}");
            // process message here
        }
    }

}