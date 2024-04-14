namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;


internal class StreamJsonContentFilter : IOperationFilter
{
    /// <summary>
    /// Configures operations decorated with the <see cref="StreamJsonContentAttribute" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // busca si tiene el atributo StreamJsonContentAttribute
        var attribute = context.MethodInfo.GetCustomAttributes(typeof(StreamJsonContentAttribute), false).FirstOrDefault();
        if (attribute == null)
        {
            return;
        }
        // agrega un parametro de body con un esquema vacio que admite propiedades adicionales
        operation.RequestBody = new OpenApiRequestBody() { Required = true };
        operation.RequestBody.Content.Add("application/json", new OpenApiMediaType()
        {
            Schema = new OpenApiSchema()
            {
                Type = "object",
                AdditionalPropertiesAllowed = true
            },
        });
    }
}

