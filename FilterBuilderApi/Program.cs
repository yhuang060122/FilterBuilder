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
            new Employee { Id = 1, Name = "Alice Smith", Age = 28, Salary = 65000m, Department = "Engineering", Active = true },
            new Employee { Id = 2, Name = "Bob Johnson", Age = 35, Salary = 85000m, Department = "Engineering", Active = true },
            new Employee { Id = 3, Name = "Charlie Brown", Age = 42, Salary = 92000m, Department = "Sales", Active = false },
            new Employee { Id = 4, Name = "Diana Prince", Age = 31, Salary = 78000m, Department = "Marketing", Active = true },
            new Employee { Id = 5, Name = "Evan Wright", Age = 24, Salary = 52000m, Department = "HR", Active = true },
            new Employee { Id = 6, Name = "Fiona Gallagher", Age = 48, Salary = 105000m, Department = "Management", Active = true }
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
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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
