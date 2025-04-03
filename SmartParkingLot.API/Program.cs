using SmartParkingLot.Application.Services;
using SmartParkingLot.Domain.Interfaces;
using SmartParkingLot.Infraestructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Register Data Store (Singleton for In-Memory)
builder.Services.AddSingleton<InMemoryDataStore>();

// Register Repositories
builder.Services.AddScoped<IParkingSpotRepository, InMemoryParkingSpotRepository>();
builder.Services.AddScoped<IDeviceRepository, InMemoryDeviceRepository>();

// Register Services
builder.Services.AddScoped<IParkingSpotService, ParkingSpotService>();
builder.Services.AddScoped<IDeviceRegistryService, DeviceRegistryService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
