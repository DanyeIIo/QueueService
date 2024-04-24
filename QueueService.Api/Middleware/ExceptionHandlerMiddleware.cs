using Newtonsoft.Json;

namespace QueueService.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Message should not be empty or Message length should not exceed 100 characters."))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var errorResponse = new { error = "Message should not be empty or Message length should not exceed 100 characters." };
                var jsonErrorResponse = JsonConvert.SerializeObject(errorResponse);

                await context.Response.WriteAsync(jsonErrorResponse);
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new { error = "An unexpected error occurred." };
                var jsonErrorResponse = JsonConvert.SerializeObject(errorResponse);

                await context.Response.WriteAsync(jsonErrorResponse);
            }
        }
    }
}
