using SmartParkingLot.API.Middleware;
using SmartParkingLot.Application.Interfaces;
using SmartParkingLot.Application.Services;
using SmartParkingLot.Domain.Interfaces;
using SmartParkingLot.Infraestructure.Persistence.Repositories;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Parking Lot API",
        Version = "v1",
        Description = "API for managing parking spots and simulated IoT device interactions."
    });
});

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Register Application Layer Services
builder.Services.AddScoped<IParkingService, ParkingService>();

// Register Infrastructure Layer Services (Repositories, etc.)
builder.Services.AddSingleton<IParkingSpotRepository, ParkingSpotRepository>(); // Singleton for in-memory
builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();       // Singleton for in-memory

// Register Bonus Features
builder.Services.AddSingleton<IRateLimiterService, RateLimiterService>(); // Singleton for in-memory rate limiter

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
