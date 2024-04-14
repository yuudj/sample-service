namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;


internal class SingleFileOperationFilter : IOperationFilter
{


    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodParams = context.MethodInfo.GetParameters();
        var isFileUploadOperation = methodParams.Any(p => p.ParameterType.FullName.Equals(typeof(IFormFile).FullName));

        if (!isFileUploadOperation) return;

        var uploadFileMediaType = new OpenApiMediaType()
        {
            Schema = new OpenApiSchema()
            {
                Type = "object",
                Properties =
                    {
                        ["uploadedFile"] = new OpenApiSchema()
                        {
                            Description = "Upload Files",
                            Type = "File",// VA CON LA PRIMERA LETRA MAYUSCULA SINO EL OPEN API GEN NO LO TOMA BIEN
                            Format = "binary",

                        }
                    },
                Required = new HashSet<string>()
                    {
                        "uploadedFile"
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
