using Microsoft.OpenApi.Models;

namespace UNAHUR.SampleService.Infra.Web.OpenApi;


// <summary>
/// Clase que represertna los settings de swagger
/// </summary>
internal class SwaggerSettings : OpenApiInfo
{
    /// <summary>
    /// Deshabilita el feature swagger de la solucion
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// COnfiguracion del OIDC para SwaggerUI
    /// </summary>
    public SwaggerOIDCSettings SwaggerUiOAuthSettings { get; set; }
    public SwaggerSettings() : base()
    {
        this.SwaggerUiOAuthSettings = new SwaggerOIDCSettings();
    }
}

internal class SwaggerOIDCSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}