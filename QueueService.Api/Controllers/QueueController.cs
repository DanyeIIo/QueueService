using Microsoft.AspNetCore.Mvc;
using QueueService.BusinessLogic.Interfaces;
using QueueService.BusinessLogic.Models;
using QueueService.BusinessLogic.Requests;
using QueueService.BusinessLogic.Services;

namespace QueueService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueServiceAsync _queueService;
        private readonly IQueueServiceAsync _queueBulkService;
        public QueueController()
        {
            _queueService = QueueServiceAsync.Instance;
            _queueBulkService = QueueBulkServiceAsync.Instance;
        }

        [HttpPost("enqueue")]
        [ProducesResponseType(typeof(QueueDataModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnqueueTask([FromBody] QueueDataRequestModel message)
        {
            var response = await _queueService.EnqueueTaskAsync(message.Message, DateTime.UtcNow);
            return Ok(response);
        }

        [HttpPost("enqueuebulk")]
        [ProducesResponseType(typeof(QueueDataModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnqueueBulkTask([FromBody] QueueDataRequestModel message)
        {
            var response = await _queueBulkService.EnqueueTaskAsync(message.Message, DateTime.UtcNow);
            return Ok(response);
        }
    }
}
