namespace UNAHUR.SampleService.Infra.Web.Cors;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


internal static class Startup
{
    private const string CorsPolicy = nameof(CorsPolicy);

    internal static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var corsSettings = config.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
        if (corsSettings == null) return services;


        return services.AddCors(opt =>

            opt.AddPolicy(CorsPolicy, policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod();


                if (corsSettings.Origins.Contains("*"))
                    policy.AllowAnyOrigin();
                else
                    policy.WithOrigins(corsSettings.Origins.ToArray())
                    .AllowCredentials(); ;

            }));
    }

    internal static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app) =>
        app.UseCors(CorsPolicy);
}
