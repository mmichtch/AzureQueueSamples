using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleQueueProcessor.Core
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly ILogger<QueueProcessor> _logger;
        private readonly QueueProcessor _queueProcessor;

        public QueueProcessor(QueueProcessor queueProcessor,ILogger<QueueProcessor> logger)
        {
            _queueProcessor = queueProcessor;
            _logger = logger;
        }

        public async Task ProcessAsync()
        {
            _logger.LogInformation($"Process started.");

            //use loop to process all messages in the injected queue

            _logger.LogInformation("Process Completed");
        }
    }
}
