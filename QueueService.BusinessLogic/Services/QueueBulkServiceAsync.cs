using QueueService.BusinessLogic.Interfaces;
using QueueService.BusinessLogic.Models;
using QueueService.BusinessLogic.Requests;
using System.Collections.Concurrent;

namespace QueueService.BusinessLogic.Services
{
    public class QueueBulkServiceAsync : IQueueServiceAsync
    {
        private static QueueBulkServiceAsync _instance;
        private static readonly object _lockObject = new object();

        private readonly ConcurrentQueue<QueueDataItemModel> _queue = 
            new ConcurrentQueue<QueueDataItemModel>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);
        private readonly Random _random = new Random();

        public static QueueBulkServiceAsync Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new QueueBulkServiceAsync();
                        }
                    }
                }
                return _instance;
            }
        }

        private QueueBulkServiceAsync()
        {

        }

        public async Task<QueueDataModel> EnqueueTaskAsync(string data, DateTime requestTime)
        {
            if(string.IsNullOrEmpty(data) || data.Length > 100)
            {
                throw new ArgumentException("Message should not be empty or Message length should not exceed 100 characters.");
            }

            var queueItem = new QueueDataItemModel { Data = data, RequestTime = requestTime };

            _queue.Enqueue(queueItem);

            var processingTasks = new List<Task<QueueDataModel>>();
            while (!_queue.IsEmpty && processingTasks.Count < 5)
            {
                if (_queue.TryDequeue(out var queuedTaskItem))
                {
                    processingTasks.Add(ProcessTask(queuedTaskItem));
                }
                else
                {
                    break;
                }
            }


            var completedTask = await Task.WhenAny(processingTasks);
            processingTasks.Remove(completedTask);

            return await completedTask;
        }

        private async Task<QueueDataModel> ProcessTask(QueueDataItemModel item)
        {
            await _semaphore.WaitAsync();

            var filePath = $"message/bulkTasks.txt";

            // Write data to file
            var requestTime = item.RequestTime;
            var formattedRequestTime = requestTime.ToString("yyyy-MM-ddTHH:mm:ss");
            var writeTime = DateTime.UtcNow;
            var processingTime = (writeTime - requestTime).TotalMilliseconds;
            var formattedWriteTime = writeTime.ToString("yyyy-MM-ddTHH:mm:ss");
            var fileContent = $"{formattedRequestTime} | {formattedWriteTime} | {item.Data}";

            lock (_lockObject)
            {
                File.AppendAllText(filePath, $"{fileContent}\n");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(100, 300)));

            // Prepare response
            var response = new QueueDataModel
            {
                RequestTime = formattedRequestTime,
                WriteTime = formattedWriteTime,
                ProcessingTime = processingTime
            };

            _semaphore.Release();

            return response;
        }
    }
}
