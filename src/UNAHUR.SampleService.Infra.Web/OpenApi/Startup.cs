using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UNAHUR.SampleService.Infra.Web.OpenApi;


internal static class Startup
{
    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();


        if (settings == null) return services;

        if (settings.Enable)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            // esto solo hace falta si se usan minimal apis
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen();

        }

        return services;
    }

    internal static WebApplication UseOpenApiDocumentation(this WebApplication app, IConfiguration config)
    {
        var settings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();

        if (settings.Enable)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {

                var apidescriptions = app.DescribeApiVersions();

                //agrega un endpoint por cada version
                foreach (var description in apidescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                    options.OAuthClientId(settings.SwaggerUiOAuthSettings.ClientId);
                    options.OAuthClientSecret(settings.SwaggerUiOAuthSettings.ClientSecret);
                }
            });


        }

        return app;
    }
}
