namespace UNAHUR.SampleService.Infra.Web.Middleware;

using Microsoft.AspNetCore.Builder;




internal static class Startup
{
    /// <summary>
    /// Registers the <see cref="RFC7807ErrorHandlerMiddleware"/> to catch all unhanled exceptions
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<RFC7807ErrorHandlerMiddleware>();


}
