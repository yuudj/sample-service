namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using UNAHUR.SampleService.Infra.Web.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;


/// <summary>
/// Agrega el atributo x-operation-name a todos los metodos. 
/// Sin este filtro ng-openapi-gen genera unos nombres muy largos https://github.com/cyclosproject/ng-openapi-gen#supported-vendor-extensions
/// </summary>
internal class XOperationNameOperationFilter : IOperationFilter
{


    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation != null)
        {
            operation.Extensions.Add("x-operation-name", new OpenApiString(context.MethodInfo.Name.ToCamelCase()));
        }


        //sac DE LA DEFINCIION SWAGGER a todas las respuestas tipo odata de las posibles respuesgtas
        // TODO: ver si con las convenciones se pueede hacer algo
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {

            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];


            foreach (var contentType in response.Content.Keys.Where(d => d.Contains("odata") || d == "application/xml" || d == "text/plain" || d == "application/octet-stream"))
            {
                // evita que se quede sin responses
                if (response.Content.Count > 1)
                    response.Content.Remove(contentType);
            }
        }
    }

}
