using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace WarehouseSystem.Configuration
{
    /// <summary>
    /// Swagger dokumentacijos konfigūracijos klasė
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Konfigūruoja Swagger servisus
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Pagrindinė API informacija
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sandėlio valdymo sistema API",
                    Version = "v1",
                    Description = "RESTful API sandėlio valdymo sistemai",
                    Contact = new OpenApiContact
                    {
                        Name = "Sistemos administratorius",
                        Email = "admin@warehouse.example.com",
                        Url = new Uri("https://warehouse.example.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Naudojimo sąlygos",
                        Url = new Uri("https://warehouse.example.com/terms")
                    }
                });

                // Grupuojame endpoint'us pagal kontrolerius
                options.TagActionsBy(api => new[] { api.GroupName });

                // Rūšiuojame endpoint'us pagal kelią
                options.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");

                // Įtraukiame XML komentarus
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Pridedame autentifikacijos nustatymus
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT autentifikacijos antraštė. Įveskite: 'Bearer {jūsų_token}'",
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Pridedame bendrus parametrus
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "X-API-Key",
                    Description = "API raktas autentifikacijai"
                });

                // Konfigūruojame dokumentų generavimo nustatymus
                options.CustomSchemaIds(type => type.FullName);
                options.EnableAnnotations();
                options.DescribeAllParametersInCamelCase();

                // Konfigūruojame atsakymų pavyzdžius
                options.ExampleFilters();

                // Pridedame operacijų filtrus
                options.OperationFilter<AddRequiredHeaderParameter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            });

            // Pridedame Swagger pavyzdžių generatorių
            services.AddSwaggerExamplesFromAssemblyOf<Program>();

            return services;
        }

        /// <summary>
        /// Konfigūruoja Swagger UI
        /// </summary>
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            // Įjungiame Swagger JSON endpoint'ą
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
                options.SerializeAsV2 = false;
            });

            // Įjungiame Swagger UI
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "Sandėlio API v1");
                options.RoutePrefix = "api-docs";
                
                // UI nustatymai
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();

                // Pritaikome custom CSS
                options.InjectStylesheet("/swagger-ui/custom.css");
                
                // Pridedame custom JavaScript
                options.InjectJavascript("/swagger-ui/custom.js");
            });

            return app;
        }
    }

    /// <summary>
    /// Filtras pridedantis reikalingas antraštes prie operacijų
    /// </summary>
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-ID",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Description = "Koreliacijos ID užklausų sekimui"
            });
        }
    }

    /// <summary>
    /// Filtras pridedantis autorizacijos informaciją prie operacijų aprašymo
    /// </summary>
    public class AppendAuthorizeToSummaryOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            if (authAttributes.Any())
            {
                operation.Summary = $"{operation.Summary} (Reikalinga autorizacija)";
                operation.Description = 
                    $"{operation.Description}\n\nŠi operacija reikalauja autentifikacijos." +
                    $"\nReikalingos rolės: {string.Join(", ", authAttributes.Select(a => a.Roles).Where(r => !string.IsNullOrEmpty(r)))}";
            }
        }
    }

    /// <summary>
    /// Konfigūracijos klasė custom CSS ir JavaScript failams
    /// </summary>
    public static class SwaggerUIConfiguration
    {
        private const string CustomCss = @"
            .swagger-ui .topbar { display: none }
            .swagger-ui .info { margin: 20px 0 }
            .swagger-ui .info .title { font-size: 24px }
            .swagger-ui .info .description { font-size: 14px }
            .swagger-ui .schemes-title { display: none }
        ";

        private const string CustomJs = @"
            window.onload = function() {
                var logo = document.createElement('img');
                logo.src = '/images/logo.png';
                logo.style.height = '40px';
                logo.style.marginRight = '10px';
                document.querySelector('.info').prepend(logo);
            }
        ";

        /// <summary>
        /// Prideda statinius failus Swagger UI
        /// </summary>
        public static IApplicationBuilder UseSwaggerUIAssets(this IApplicationBuilder app)
        {
            var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var swaggerUIPath = Path.Combine(webRootPath, "swagger-ui");

            if (!Directory.Exists(swaggerUIPath))
                Directory.CreateDirectory(swaggerUIPath);

            var cssPath = Path.Combine(swaggerUIPath, "custom.css");
            var jsPath = Path.Combine(swaggerUIPath, "custom.js");

            File.WriteAllText(cssPath, CustomCss);
            File.WriteAllText(jsPath, CustomJs);

            app.UseStaticFiles();

            return app;
        }
    }
}
