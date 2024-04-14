namespace UNAHUR.SampleService.Infra.Web.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Global error handler middleware wich follows the https://www.rfc-editor.org/rfc/rfc7807
/// </summary>
internal class RFC7807ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _envirioment;
    private readonly ILogger _log;
    /// <summary>
    /// Initializes a new instance of <see cref="RFC7807ErrorHandlerMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="env">The <see cref="IHostEnvironment"/> to check envirioment variables.</param>
    /// <param name="log">The <see cref="ILogger"/> to log when other middleware starts, finishes and throws.</param>
    public RFC7807ErrorHandlerMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<RFC7807ErrorHandlerMiddleware> log)
    {
        this._next = next;
        this._envirioment = env;
        this._log = log;
    }

    /// <summary>
    /// Executes the middleware that catches a <see cref="Exception"/> when the next middleware finishes and wraps in an http error following https://www.rfc-editor.org/rfc/rfc7807
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            var traceId = string.IsNullOrEmpty(context.TraceIdentifier) ? Guid.NewGuid().ToString() : context.TraceIdentifier;


            switch (error)
            {
                case FluentValidation.ValidationException e:
                    // maneja los errores de validacion
                    var problemDetails = new ValidationProblemDetails()
                    {

                        Instance = context.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = e.Message ?? "Validation errors occured please on the request sent",
                        Title = "Validation Error",
                        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",

                    };
                    // agrupa los errores por propiedad y los agrega al detalle
                    foreach (var errorGropup in e.Errors.GroupBy(e => e.PropertyName))
                    {
                        problemDetails.Errors.Add(errorGropup.Key, errorGropup.Select(e => e.ErrorMessage).ToArray());
                    }


                    context.Response.StatusCode = (int)StatusCodes.Status400BadRequest;



                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problemDetails), context.RequestAborted);


                    break;
                default:

                    // todo: verificar que este formato funcione
                    _log.LogError(error, "Unhandleld exception in {Path}", context.Request.Path);

                    // unhandled error
                    var errorResponse = new JsonErrorResponse
                    {
                        Instance = context.Request.Path,
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Ha ocurrido un error al procesar el HttpRequest",
                        Detail = error.Message,
                        TraceId = traceId
                    };

                    var list = new List<string>
                        {
                            error.Message
                        };

                    if (_envirioment.IsDevelopment())
                    {
                        list.Add(error.StackTrace);
                    }
                    errorResponse.Errors = list.ToArray();

                    context.Response.StatusCode = (int)StatusCodes.Status500InternalServerError;


                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse), context.RequestAborted);
                    break;
            }
        }
    }
    private class JsonErrorResponse : ProblemDetails
    {
        public string[] Errors { get; set; }
        public string TraceId { get; set; }
    }


}
