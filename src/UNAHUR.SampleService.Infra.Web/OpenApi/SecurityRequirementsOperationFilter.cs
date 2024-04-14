namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Filtro de swagger que detecta aquellos controladores con el atributo [Authorize]
/// y les genera las respuestas 401, 403 y les agrega una referencia al esquema de authorizacion
/// </summary>
internal class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Policy names map to scopes
        var methodScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct();

        var classScopes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct();

        var requiredScopes = methodScopes.Union(classScopes);

        if (requiredScopes.Any())
        {
            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            if (!operation.Responses.ContainsKey("403"))
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });


            var oAuthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };

            operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ oAuthScheme ] = requiredScopes.Where(x=> !string.IsNullOrEmpty(x)).ToList()
                    }
                };
        }
    }
}
