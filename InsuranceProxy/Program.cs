using InsuranceProxy.Interfaces;
using InsuranceProxy.Models;
using InsuranceProxy.Services;
using InsuranceProxy.Tools;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var companies = builder.Configuration
    .GetSection("McpProxy:Companies")
    .Get<List<CompanyServerConfig>>() ?? [];

var jwtIssuer = builder.Configuration["McpProxy:JwtIssuer"] ?? "InsuranceProxy";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT token service
builder.Services.AddSingleton(new JwtTokenService(jwtIssuer));

// Proxy services
builder.Services.AddSingleton<ICompanyClientRegistry, CompanyClientRegistry>();
builder.Services.AddSingleton<IToolRouter, ToolRouter>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ProxyTool>();

var app = builder.Build();

app.UseCors();

var registry = app.Services.GetRequiredService<ICompanyClientRegistry>();
foreach (var company in companies.Where(c => c.IsEnabled))
{
    try
    {
        await registry.RegisterAsync(company);
        app.Logger.LogInformation("Connected to {DisplayName}", company.DisplayName);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not connect to {DisplayName} — skipping", company.DisplayName);
    }
}

app.MapMcp("/mcp");

await app.RunAsync();