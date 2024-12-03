using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WarehouseSystem.Services;
using WarehouseSystem.Services.Interfaces;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace WarehouseSystem.Extensions
{
    /// <summary>
    /// Plėtinių klasė servisų registracijai
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registruoja visus sistemos servisus
        /// </summary>
        public static IServiceCollection AddWarehouseServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registruojame pagrindinius servisus
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IMovementService, MovementService>();
            services.AddScoped<IQRCodeService, QRCodeService>();

            // Registruojame papildomus servisus jei reikia
            services.AddLogging();
            services.AddMemoryCache();

            // Registruojame Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sandėlio valdymo sistema",
                    Version = "v1",
                    Description = "API dokumentacija sandėlio valdymo sistemai",
                    Contact = new OpenApiContact
                    {
                        Name = "Sistemos administratorius",
                        Email = "admin@example.com"
                    }
                });

                // Įtraukiame XML komentarus
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Pridedame autentifikacijos nustatymus
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT autentifikacijos antraštė naudojant Bearer schemą. Pavyzdys: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// Registruoja CORS politiką
        /// </summary>
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", builder =>
                {
                    builder.WithOrigins(
                            configuration.GetSection("AllowedOrigins").Get<string[]>() 
                            ?? new[] { "http://localhost:3000" }
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Registruoja validacijos servisus
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            // Įtraukiame Fluent Validation
            services.AddFluentValidation(fv =>
            {
                // Registruojame validatorius iš assembly
                fv.RegisterValidatorsFromAssemblyContaining<Program>();
                fv.AutomaticValidationEnabled = true;
                fv.ImplicitlyValidateChildProperties = true;
            });

            return services;
        }

        /// <summary>
        /// Registruoja duomenų saugojimo servisus
        /// </summary>
        public static IServiceCollection AddStorageServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Nustatome bazinį kelią iki duomenų katalogo
            var dataPath = configuration.GetValue<string>("DataPath") 
                ?? Path.Combine(AppContext.BaseDirectory, "Data");
            
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            // Registruojame kelią kaip konfigūracijos opciją
            services.Configure<StorageOptions>(options =>
            {
                options.DataPath = dataPath;
                options.ProductsFilePath = Path.Combine(dataPath, "products.csv");
                options.LocationsFilePath = Path.Combine(dataPath, "locations.csv");
                options.MovementsFilePath = Path.Combine(dataPath, "movements.csv");
                options.QRCodesFilePath = Path.Combine(dataPath, "qr_codes.csv");
            });

            return services;
        }

        /// <summary>
        /// Registruoja verslo taisyklių validatorius
        /// </summary>
        public static IServiceCollection AddBusinessValidators(this IServiceCollection services)
        {
            // Čia galima pridėti custom validatorius
            return services;
        }

        /// <summary>
        /// Registruoja darbo su CSV failais servisus
        /// </summary>
        public static IServiceCollection AddCsvServices(this IServiceCollection services)
        {
            services.AddSingleton<ICsvFileManager, CsvFileManager>();
            services.AddSingleton<ICsvParser, CsvParser>();
            
            return services;
        }
    }

    /// <summary>
    /// Saugyklos kelių nustatymai
    /// </summary>
    public class StorageOptions
    {
        public string DataPath { get; set; }
        public string ProductsFilePath { get; set; }
        public string LocationsFilePath { get; set; }
        public string MovementsFilePath { get; set; }
        public string QRCodesFilePath { get; set; }
    }

    /// <summary>
    /// CSV failų valdymo interfeisas
    /// </summary>
    public interface ICsvFileManager
    {
        Task<List<string>> ReadAllLinesAsync(string filePath);
        Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines);
        Task AppendLineAsync(string filePath, string line);
    }

    /// <summary>
    /// CSV failų valdymo implementacija
    /// </summary>
    public class CsvFileManager : ICsvFileManager
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<List<string>> ReadAllLinesAsync(string filePath)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var reader = new StreamReader(filePath);
                var lines = new List<string>();
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
                return lines;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines)
        {
            await _semaphore.WaitAsync();
            try
            {
                await File.WriteAllLinesAsync(filePath, lines);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AppendLineAsync(string filePath, string line)
        {
            await _semaphore.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    /// <summary>
    /// CSV analizatoriaus interfeisas
    /// </summary>
    public interface ICsvParser
    {
        string[] ParseLine(string line);
        string FormatLine(string[] values);
        string EscapeField(string field);
    }

    /// <summary>
    /// CSV analizatoriaus implementacija
    /// </summary>
    public class CsvParser : ICsvParser
    {
        public string[] ParseLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var field = new System.Text.StringBuilder();

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            result.Add(field.ToString());
            return result.ToArray();
        }

        public string FormatLine(string[] values)
        {
            return string.Join(",", values.Select(v => 
                v.Contains(",") || v.Contains("\"") || v.Contains("\n") 
                    ? $"\"{EscapeField(v)}\""
                    : v
            ));
        }

        public string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }
}
