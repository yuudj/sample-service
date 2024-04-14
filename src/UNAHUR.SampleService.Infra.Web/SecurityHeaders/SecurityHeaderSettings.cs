namespace UNAHUR.SampleService.Infra.Web.SecurityHeaders;

public class SecurityHeaderSettings
{
    public bool Enable { get; set; }
    public SecurityHeaders Headers { get; set; } = default!;
}
