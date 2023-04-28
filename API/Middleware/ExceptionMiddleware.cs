using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        public RequestDelegate Next { get; }
        public ILogger<ExceptionMiddleware> Logger { get; }
        public IHostEnvironment Env { get; }
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            this.Env = env;
            this.Logger = logger;
            this.Next = next;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.Next(context);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var response = this.Env.IsDevelopment()
                    ? new APiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new APiException(context.Response.StatusCode, ex.Message, "Internal Server Error");
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);

            }
        }


    }
}