using HeyMom.Api.Features;
using HeyMom.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' must be configured.");

builder.Services.AddDbContext<HeyMomDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddFeatures();

var app = builder.Build();
app.UseExceptionHandler();
app.MapFeatures();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/api/openapi/{documentName}.json");
}

app.Run();

public partial class Program;
