namespace LinQHomework.Middlewares
{
    using System.Diagnostics;
    using Middlewares.Interfaces;



    public class ResponseTimeMiddleware
    {
        private readonly RequestDelegate _next;
        public ResponseTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
                          IMiddlewareResponseTimeStopwatch watch)
        {
            watch.Start();

            context.Response.OnStarting(state =>
            {
                watch.Stop();
                string value = string.Format("{0}ms", watch.ElapsedMilliseconds);
                context.Response.Headers["X-Response-Time"] = value;
                return Task.CompletedTask;
            }, context);
            await _next(context);
        }
    }

    public interface IMiddlewareResponseTimeStopwatch : IStopwatch
    {
    }

    public class MiddlewareResponseTimeStopwatch : Stopwatch,
                                   IMiddlewareResponseTimeStopwatch
    {
        public MiddlewareResponseTimeStopwatch() : base()
        {
        }
    }
}
