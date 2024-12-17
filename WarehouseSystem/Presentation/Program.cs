using Application.Extensions;
using Application.Services;
using Application.Services.Interfaces;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using ICurrentUserService = Application.Services.Interfaces.ICurrentUserService;
using CurrentUserService = Application.Services.CurrentUserService;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build(); // Add this line to create the 'app' variable

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
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();