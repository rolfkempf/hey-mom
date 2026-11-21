using System.Net;
using HeyMom.Api.Features;
using HeyMom.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' must be configured.");

builder.Services.AddDbContext<HeyMomDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddFeatures(builder.Configuration, builder.Environment);

var trustedProxy = builder.Configuration["Authentication:TrustedProxy"];
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    if (trustedProxy is not null)
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        options.KnownProxies.Add(IPAddress.Parse(trustedProxy));
    }
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapFeatures();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/api/openapi/{documentName}.json");
}

app.Run();

public partial class Program;
