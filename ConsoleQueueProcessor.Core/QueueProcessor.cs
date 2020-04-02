using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace ConsoleQueueProcessor.Core
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly DocumentQueues _queues;
        private readonly ILogger<QueueProcessor> _logger;

        public QueueProcessor(DocumentQueues queues, ILogger<QueueProcessor> logger)
        {
            _queues = queues;
            _logger = logger;
        }

        public async Task ProcessAsync()
        {
            _logger.LogInformation($"Process started.");

            var message = await _queues.SourceQueue.GetMessageAsync();
            while (message != null)
            {

                try
                {
                    DoProcess(message);
                    await _queues.SourceQueue.DeleteMessageAsync(message);
                    await _queues.SuccessQueue.AddMessageAsync(new CloudQueueMessage(message.AsBytes));
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    if(message.DequeueCount > 2)
                    {
                        await _queues.ErrorQueue.AddMessageAsync(new CloudQueueMessage(message.AsBytes));
                        await _queues.SourceQueue.DeleteMessageAsync(message);
                        _logger.LogInformation($"message moved to error queue.");
                    }
                }

                message = await _queues.SourceQueue.GetMessageAsync();
            }

            _logger.LogInformation("Process Completed");
        }

        private void DoProcess(CloudQueueMessage message)
        {
            var value = message.AsString;
            _logger.LogInformation($"Message: {value}");
            if (value.StartsWith("error"))
            {
                throw new Exception($"Message {value} is faulty.");
            }
            else
            {
                _logger.LogInformation($"Message {value} processed succesfully");
            }
        }
    }
}
