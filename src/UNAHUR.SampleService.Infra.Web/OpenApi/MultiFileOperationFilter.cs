namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Filtro de operaciones API que suben mas de un archivo 
/// </summary>
/// <remarks>
/// Las operaciones que suben mas de un archivo se detectan por que poseeun parametro del tipo <see cref="IFormFileCollection"/>
/// </remarks>
internal class MultiFileOperationFilter : IOperationFilter
{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodParams = context.MethodInfo.GetParameters();
        var isFileUploadOperation = methodParams.Any(p => p.ParameterType.FullName.Equals(typeof(IFormFileCollection).FullName));

        if (!isFileUploadOperation) return;

        var uploadFileMediaType = new OpenApiMediaType()
        {
            Schema = new OpenApiSchema()
            {
                Type = "object",
                Properties =
                    {
                        ["uploadedFiles"] = new OpenApiSchema()
                        {
                            Description = "Upload Files",
                            Type = "array",
                            Items= new OpenApiSchema()
                            {
                                Type = "string", // VA CON LA PRIMERA LETRA MAYUSCULA SINO EL OPEN API GEN NO LO TOMA BIEN
                                Format = "binary",
                            },
                        }
                    },
                Required = new HashSet<string>()
                    {
                        "uploadedFiles"
                    }
            }
        };
        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
                {
                    ["multipart/form-data"] = uploadFileMediaType
                }
        };
    }
}
