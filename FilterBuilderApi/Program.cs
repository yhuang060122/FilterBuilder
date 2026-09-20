using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FilterBuilderDb"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed initial employee data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Employees.Any())
    {
        db.Employees.AddRange(
            new Employee { Id = 1, Name = "Alice Smith 28", Age = 28, Salary = 65028m, Department = "Engineering", Active = true },
            new Employee { Id = 2, Name = "Bob Johnson", Age = 35, Salary = 85000m, Department = "Engineering", Active = true },
            new Employee { Id = 3, Name = "Charlie Brown", Age = 42, Salary = 92000m, Department = "Sales", Active = false },
            new Employee { Id = 4, Name = "Diana Prince", Age = 31, Salary = 78000m, Department = "Marketing", Active = true },
            new Employee { Id = 5, Name = "Evan Wright", Age = 24, Salary = 52000m, Department = "HR", Active = true },
            new Employee { Id = 6, Name = "Fiona Gallagher", Age = 48, Salary = 105000m, Department = "Management", Active = true }
        );
        db.SaveChanges();
    }

    if (!db.UniverseVersions.Any())
    {
        var alpha = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var beta = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var gamma = Guid.Parse("33333333-3333-3333-3333-333333333333");

        db.UniverseVersions.AddRange(
            new(alpha, 1, new DateTime(2026, 1, 1), "Alpha V1"),
            new(alpha, 2, new DateTime(2026, 2, 1), "Alpha V2"),
            new(alpha, 3, new DateTime(2026, 3, 1), "Alpha V3"),

            new(beta, 1, new DateTime(2026, 1, 10), "Beta V1"),
            new(beta, 2, new DateTime(2026, 4, 10), "Beta V2"),

            new(gamma, 1, new DateTime(2026, 2, 15), "Gamma V1")
        );

        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
