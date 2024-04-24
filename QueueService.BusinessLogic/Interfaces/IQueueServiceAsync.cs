using QueueService.BusinessLogic.Models;

namespace QueueService.BusinessLogic.Interfaces
{
    public interface IQueueServiceAsync
    {
        Task<QueueDataModel> EnqueueTaskAsync(string data, DateTime requestTime);
    }
}
