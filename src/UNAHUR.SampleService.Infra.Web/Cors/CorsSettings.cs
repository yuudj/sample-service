namespace UNAHUR.SampleService.Infra.Web.Cors;

using System.Collections.Generic;
/// <summary>
/// Clase de configuracion del cross-origin resource sharing
/// </summary>
internal class CorsSettings
{
    /// <summary>
    /// Lista de URLs de origen permitidas. Si especifica '*' se admite cualquier origen
    /// </summary>
    public List<string> Origins { get; set; }

    public CorsSettings()
    {
        Origins = new List<string>();
    }
}
