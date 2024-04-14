namespace UNAHUR.SampleService.Infra.Web.OpenApi;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using UNAHUR.IoT.Shared.Web.Utils;


/// <summary>
/// 
/// </summary>
internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;
    private readonly SwaggerSettings settings;
    private readonly JwtBearerOptions jwtOptions;
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration config)
    {
        this.provider = provider;
        this.settings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
        this.jwtOptions = config.GetSection(nameof(JwtBearerOptions)).Get<JwtBearerOptions>();

    }
    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        //https://github.com/dotnet/aspnet-apiDescription-versioning/wiki/Swashbuckle-Integration
        foreach (var description in provider.ApiVersionDescriptions)
        {
            settings.Version = description.ApiVersion.ToString();

            options.SwaggerDoc(description.GroupName, settings);

        }

        // Predicado de inclusion a swagger
        options.DocInclusionPredicate((documentName, apiDescription) =>
        {
            // excluye todo lo que empieza con odata
            if (apiDescription.RelativePath.Contains("odata"))
                return false;


            // esto es necesario para que filtre la version (https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/8f363f7359cb1cb8fa5de5195ec6d97aefaa16b3/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGeneratorOptions.cs#L70)

            return apiDescription.GroupName == null || apiDescription.GroupName == documentName;

        });

        var authUrl = StringHelpers.UrlCombine(jwtOptions.Authority, "/protocol/openid-connect/auth");
        var tokenhUrl = StringHelpers.UrlCombine(jwtOptions.Authority, "/protocol/openid-connect/token");

        // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {

                    AuthorizationUrl = new Uri(authUrl),
                    TokenUrl = new Uri(tokenhUrl),
                    Scopes = new Dictionary<string, string>
                            {
                                { "openid", "openid" },
                                { "profile", "profile" }
                            }
                }
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "openid","profile" }
                    }
                });


        options.OperationFilter<AddApiVersionMetadata>();

        options.OperationFilter<StreamJsonContentFilter>();


        options.OperationFilter<XOperationNameOperationFilter>();

        // Assign scope requirements to operations based on AuthorizeAttribute
        options.OperationFilter<SecurityRequirementsOperationFilter>();

        // filtro para que el swagger genere bien los uploads de archivo
        options.OperationFilter<SingleFileOperationFilter>();

        // filtro para que el swagger genere bien los uploads de multiples archivos
        options.OperationFilter<MultiFileOperationFilter>();

        options.DescribeAllParametersInCamelCase();
    }
}
