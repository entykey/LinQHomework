﻿namespace LinQHomework.Filters
{
    using LinQHomework.Middlewares.Interfaces;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Diagnostics;



    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ResponseTimeFilter : Attribute, IActionFilter
    {
        private IActionResponseTimeStopwatch GetStopwatch(HttpContext context)
        {
            return context.RequestServices.GetService<IActionResponseTimeStopwatch>();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            IStopwatch watch = GetStopwatch(context.HttpContext);
            watch.Reset();
            watch.Start();
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            IStopwatch watch = GetStopwatch(context.HttpContext);
            watch.Stop();
            string value = string.Format("{0}ms", watch.ElapsedMilliseconds);
            context.HttpContext.Response.Headers["X-Action-Response-Time"] = value;
        }
    }

    public interface IActionResponseTimeStopwatch : IStopwatch
    {
    }

    public class ActionResponseTimeStopwatch : Stopwatch, IActionResponseTimeStopwatch
    {
        public ActionResponseTimeStopwatch() : base()
        {
        }
    }
}
