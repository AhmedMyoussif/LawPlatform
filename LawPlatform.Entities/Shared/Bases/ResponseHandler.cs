using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace LawPlatform.Entities.Shared.Bases
{
    public class ResponseHandler
    {
        public Response<T> Deleted<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = Message
            };
        }
        public Response<T> Success<T>(T entity, string message)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message
            };
        }
        public Response<T> Unauthorized<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = Message
            };
        }
        public Response<T> Forbidden<T>(string message = null)
        {
            return new Response<T>
            {
                StatusCode = HttpStatusCode.Forbidden,
                Succeeded = false,
                Message = message
            };
        }
        public Response<T> BadRequest<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message
            };
        }
        public Response<T> UnprocessableEntity<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = Message
            };
        }
        public Response<T> NotFound<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message
            };
        }
        public Response<T> Created<T>(T entity, string message = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = message
            };
        }
        public Response<T> ServerError<T>(string message = "An unexpected error occurred.")
        {
            return new Response<T>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Succeeded = false,
                Message = message,
            };
        }
        public Response<T> InternalServerError<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Succeeded = false,
                Message = message
            };
        }
        public IActionResult HandleModelStateErrors(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
            return new BadRequestObjectResult(new Response<object>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Succeeded = false,
                Errors = errors,
                Message = "There is an error while operation please try again"
            });
        }
        
        public Response<T> ValidationError<T>(List<string> errors)
        {
            return new Response<T>
            {
                Succeeded = false,
                Message = "Validation failed",
                Errors = errors
            };
        }
    }
}
