using Application.Extensions;
using Application.Services;
using Application.Services.Interfaces;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using ICurrentUserService = Domain.Interfaces.ICurrentUserService;
using CurrentUserService = Application.Services.CurrentUserService;

var builder = WebApplication.CreateBuilder(args);

// Get the configuration value for users file path
var usersFilePath = builder.Configuration.GetValue<string>("FileStorage:UsersFilePath")
    ?? Path.Combine(builder.Environment.ContentRootPath, "Data", "users.csv");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bootstrap application services
builder.Services.BootstrapApplication();

// Infrastructure services
builder.Services.AddScoped<IAuditService, AuditService>();

// Application services
builder.Services.AddScoped<ICustomersService, CustomersService>();
builder.Services.AddScoped<ISitesService, SitesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Infrastructure repositories
builder.Services.AddSingleton<CsvFileService>();
builder.Services.AddScoped<ICustomerRepository, CsvCustomerRepository>();
builder.Services.AddScoped<ISiteRepository, CsvSiteRepository>();
builder.Services.AddScoped<IUserRepository>(sp => new UserRepository(usersFilePath));

// Build the application AFTER registering all services
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Add this line for HTTPS redirection
app.UseRouting();        // Add this line for routing
app.UseCors();          // Add this if you need CORS support

app.UseAuthorization();
app.MapControllers();
app.Run();