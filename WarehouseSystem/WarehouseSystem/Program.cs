using Microsoft.OpenApi.Models;
using System.Reflection;
using Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Pridedame kontrolerius
builder.Services.AddControllers();

// Pridedame Swagger/OpenAPI konfigūraciją
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Elektros Prekių Sandėlio API",
        Version = "v1",
        Description = "API sandėlio valdymo sistemai su produktų, klientų ir objektų valdymu",
        Contact = new OpenApiContact
        {
            Name = "Sistemos Administratorius",
            Email = "admin@example.com"
        }
    });

    // Įtraukiame XML komentarus į Swagger dokumentaciją
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Registruojame aplikacijos servisus
builder.Services.BootstrapApplication();

// CORS konfigūracija React aplikacijai
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000") // React aplikacijos adresas
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Konfigūruojame HTTP užklausų pipeline

// Įjungiame Swagger tik development aplinkoje
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elektros Prekių Sandėlio API V1");
        c.RoutePrefix = "swagger";
        // Konfigūruojame Swagger UI išvaizdą
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.DisplayOperationId();
    });
}

// Įjungiame CORS
app.UseCors("ReactAppPolicy");

// Įjungiame routing
app.UseRouting();

// Įjungiame autorizaciją (jei bus naudojama ateityje)
app.UseAuthorization();

// Maršrutizuojame į kontrolerius
app.MapControllers();

app.Run();
