using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Services;
using MCPDemo.Infrastructure.ExternalApi;
using MCPDemo.Infrastructure.Logging;
using MCPDemo.Infrastructure.Metrics;
using ModelContextProtocol;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configure Serilog
// Initialize the global logger using the configuration defined in Infrastructure
Log.Logger = SerilogConfiguration.CreateConfiguration().CreateLogger();
builder.Services.AddSerilog();

// 2. Register Application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// 3. Register Infrastructure services
builder.Services.AddHttpClient<IPlatziStoreApiClient, PlatziStoreApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.escuelajs.co/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<IMetricsCollector, InMemoryMetricsCollector>();

// 4. Register and configure the MCP Server
// Automatically discovers tool methods decorated with [McpServerTool]
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

try
{
    Log.Information("Starting MCP Server...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MCP Server terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
