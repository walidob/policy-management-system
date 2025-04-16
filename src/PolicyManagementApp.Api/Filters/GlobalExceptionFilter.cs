using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolicyManagementApp.Api.Models.ApiModels;
using Serilog;

namespace PolicyManagementApp.Api.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Log.Error(context.Exception, 
                "An unhandled exception occurred in controller action: {Action}", 
                context.ActionDescriptor.DisplayName);

            var errorResponse = new ErrorResponseModel
            {
                Error = "An error occurred while processing your request.",
                StatusCode = 500
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = 500
            };

            context.ExceptionHandled = true;
        }
    }
} 