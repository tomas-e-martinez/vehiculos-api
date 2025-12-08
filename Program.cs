using Microsoft.EntityFrameworkCore;
using vehiculos_api.Data;
using vehiculos_api.Model;
using vehiculos_api.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<VehicleContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddTransient<UsersService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleContext>();
    db.Database.Migrate();

    if(!db.VehicleTypes.Any())
    {
        Console.WriteLine("VehicleTypes sin registros, aplicando seed...");

        db.VehicleTypes.AddRange(
            new VehicleType { Name = "Auto", Description = "Vehículo con 4 ruedas." },
            new VehicleType { Name = "Moto", Description = "Vehículo con 2 ruedas." }
        );
        db.SaveChanges();
    }

    if(!db.MaintenanceTypes.Any())
    {
        Console.WriteLine("MaintenanceTypes sin registros, aplicando seed...");

        var auto = db.VehicleTypes.First(v => v.Name == "Auto");
        var moto = db.VehicleTypes.First(v => v.Name == "Moto");

        db.MaintenanceTypes.AddRange(
            new MaintenanceType { 
                Name = "Cambio de aceite", 
                Description = "Cambio de aceite y filtro",
                DefaultKmInterval = 10000,
                DefaultMonthInterval = 12,
                VehicleTypes = new List<VehicleType> { auto, moto }
            }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
