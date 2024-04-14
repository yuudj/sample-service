namespace UNAHUR.SampleService.Infra.Web.Utils;
using System.Net.Http;

/// <summary>
/// Deshabilita la validacion de certificados HTTPS
/// </summary>
internal class DevelopmentBackchannelHttpHandler : HttpClientHandler
{
    public DevelopmentBackchannelHttpHandler() : base()
    {
        this.ServerCertificateCustomValidationCallback = delegate { return true; };
    }
}

