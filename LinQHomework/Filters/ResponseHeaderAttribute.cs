namespace LinQHomework.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ResponseHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;

        // this is defining the attribute usage: [ResponseHeader("Filter-Header", "Filter Value")]
        public ResponseHeaderAttribute(string name, string value)
        {
            (_name, _value) = (name, value);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // before the action executes :
            context.HttpContext.Response.Headers.Add(_name, _value);
            Console.WriteLine("[ResponseHeaderAttribute] OnActionExecuting...");

            base.OnResultExecuting(context);
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //To do : after the action executes  
            Console.WriteLine("[ResponseHeaderAttribute] OnActionExecuted!");
        }
    }
}
