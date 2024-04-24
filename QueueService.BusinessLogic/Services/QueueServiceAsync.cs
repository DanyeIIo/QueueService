using QueueService.BusinessLogic.Interfaces;
using System.Collections.Concurrent;
using QueueService.BusinessLogic.Requests;
using QueueService.BusinessLogic.Models;

namespace QueueService.BusinessLogic.Services
{
    public class QueueServiceAsync : IQueueServiceAsync
    {
        private static readonly object _lockObject = new object();
        private static QueueServiceAsync _instance;

        private readonly ConcurrentQueue<QueueDataItemModel> _queue =
            new ConcurrentQueue<QueueDataItemModel>();
        private readonly Random _random = new Random();

        public static QueueServiceAsync Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new QueueServiceAsync();
                        }
                    }
                }
                return _instance;
            }
        }

        private QueueServiceAsync()
        {

        }

        public async Task<QueueDataModel> EnqueueTaskAsync(string data, DateTime requestTime)
        {
            if (string.IsNullOrEmpty(data) || data.Length > 100)
            {
                throw new ArgumentException("Message should not be empty or Message length should not exceed 100 characters.");
            }

            var queueData = new QueueDataItemModel { Data = data, RequestTime = requestTime };
            _queue.Enqueue(queueData);
            var response = await ProcessTask();
            return response;
        }

        private async Task<QueueDataModel> ProcessTask()
        {
            DateTime writeTime;
            string formattedRequestTime;
            string formattedWriteTime;
            double processingTime;
            string filePath = $"message/tasks.txt";
            string fileContent;

            // Write data to file
            lock (_lockObject)
            {
                _queue.TryDequeue(out var data);
                var requestTime = data.RequestTime;
                formattedRequestTime = requestTime.ToString("yyyy-MM-ddTHH:mm:ss");
                writeTime = DateTime.UtcNow;
                processingTime = (writeTime - requestTime).TotalMilliseconds;
                formattedWriteTime = writeTime.ToString("yyyy-MM-ddTHH:mm:ss");
                fileContent = $"{formattedRequestTime} | {formattedWriteTime} | {data.Data}";

                File.AppendAllText(filePath, $"{fileContent}\n");
            }
            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(50, 1000)));

            // Prepare response
            var response = new QueueDataModel
            {
                RequestTime = formattedRequestTime,
                WriteTime = formattedWriteTime,
                ProcessingTime = processingTime
            };

            return response;
        }
    }
}
