using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace UNAHUR.SampleService.Api;

using UNAHUR.SampleService.Infra.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using UNAHUR.SampleService.Infra.Observability;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting;

public static class Program
{
    public static void Main(string[] args)
    {

        // REGISTRO DE SERVICIOS DE LA APLICACION
        // ver https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/PII
        //IdentityModelEventSource.ShowPII = true;

        // CREAR EL APP BUILDER,ES EL ENCARGADO DE CONFIGURAR LA INYECCION DE DEPENDENCIAS
        // Y LOS ORIGENES DE CONFIGURACION
        var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            builder.Host.UseSystemd();
            
        }
        else {
            builder.Host.UseWindowsService();
        }
#endif
        

        var services = builder.Services;
        var config = builder.Configuration;

        builder.AddConfigurations();
        builder.ConfigureOpenTelemetryAPI("UNAHUR.SampleService.Api", typeof(Program).Assembly.GetName().Version?.ToString(), Environment.MachineName);


        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                // arregla un problema con los enums en OpenApi, los define por nombre
                // Tambien admite numero pero ATENCION admite valores enteros fuera de los enums
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-586284629
                // 
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // ayuda a convertir el LTREE en json
                //options.JsonSerializerOptions.Converters.Add(new SqlHierarchyIdJsonConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            }).AddOData(opt =>
            {
                opt.AddRouteComponents("odata/v1", SetupOData.GetEdmModel_V1());
                // habilita las funciones en ODATA 
                opt.Select().Filter().OrderBy().Expand();
                // si se debe hacer un cambio drastico al ODATA se puede hacer en una version 2 simultanea
                //opt.AddRouteComponents("odata/v2", SetupOData.GetEdmModel_V2());

            });

        services.ConfigureInfrastructure(config);

        services.AddInfrastructure(config);


        // COMO REGISTRAR LOS HEALTHECHS ?
        services.AddHealthChecks();

        WebApplication app = builder.Build();

        // MIDDLEWARE
        app.UseInfrastructure(app.Configuration);

        app.MapEndpoints();


        app.Run();
    }


    /// <summary>
    /// Mapea los endpoints de los controladores y los HealthChecks
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers()
            .RequireAuthorization();

        // TODO: REVISAR BIEN
        builder.MapHealthChecks("/healthz/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        { Predicate = r => false });

        // TIENEN QUE ESTA TAGUEADOS CON "ready"
        builder.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Any(t => t.ToLowerInvariant().Equals("ready"))
        });

        /// TODO : FALTA ODATA
        return builder;
    }
}

