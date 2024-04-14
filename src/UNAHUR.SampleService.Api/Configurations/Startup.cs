namespace UNAHUR.SampleService.Api;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

internal static class Startup
{
    /// <summary>
    /// directorio de configuracion
    /// </summary>
    const string configurationsDirectory = "Configurations";

    /// <summary>
    /// Agrega los origenes de configuracion necesarios
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static IHostApplicationBuilder AddConfigurations(this IHostApplicationBuilder builder)
    {

        var env = builder.Environment;
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"business", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddJsonFile($"logger", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddJsonFile($"cors", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddJsonFile($"openapidata", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddJsonFile($"security", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddJsonFile($"securityheaders", env.EnvironmentName, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        return builder;
    }

    /// <summary>
    /// Carga una archivo de configuracion desde el directorio <see cref="configurationsDirectory"/>
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="name"></param>
    /// <param name="environmentName"></param>
    /// <param name="optional"></param>
    /// <param name="reloadOnChange"></param>
    /// <returns></returns>
    private static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder configuration, string name, string environmentName, bool optional, bool reloadOnChange)
    {
        return configuration
                .AddJsonFile($"{configurationsDirectory}/{name}.json", optional, reloadOnChange)
                .AddJsonFile($"{configurationsDirectory}/{name}.{environmentName}.json", true, reloadOnChange);
    }
}
