# Quickstart: MCP Server & C# Tool Definitions

**Feature**: `3-mcp-server-tools`  
**Date**: 2026-03-10

## Prerequisites

- .NET 8 SDK installed
- Phase 1 and Phase 2 complete (solution builds with zero errors)
- NuGet packages: `ModelContextProtocol`, `Microsoft.Extensions.Hosting`, `Serilog.AspNetCore`

## Setup Steps

### 1. Install NuGet Packages

```bash
cd src/MCPDemo.Api
dotnet add package ModelContextProtocol
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Serilog.AspNetCore
```

### 2. Create Tool Files

Create the following files in `src/MCPDemo.Api/McpTools/`:
- `ProductTools.cs` — 8 static methods for product operations
- `CategoryTools.cs` — 7 static methods for category operations
- `SearchTools.cs` — 1 static method for product search

### 3. Bootstrap the MCP Server

Replace the placeholder `Program.cs` with the MCP server bootstrap:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;

var builder = Host.CreateApplicationBuilder(args);

// Register Application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// Register Infrastructure services
builder.Services.AddHttpClient<IPlatziStoreApiClient, PlatziStoreApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.escuelajs.co/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddSingleton<IMetricsCollector, InMemoryMetricsCollector>();

// Configure Serilog
// ... (see SerilogConfiguration)

// Register MCP server with stdio transport
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
```

### 4. Build and Verify

```bash
dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true
```

Expected output: **Build succeeded, 0 errors, 0 warnings**.

### 5. Run the MCP Server

```bash
dotnet run --project src/MCPDemo.Api
```

The server will listen on stdio for JSON-RPC tool calls from Cursor IDE.

## Key Verification Points

| Check | How to Verify |
|-------|---------------|
| All 16 tools registered | Tool listing response returns 16 tools |
| Product CRUD works | `get_product_by_id` with ID 1 returns JSON |
| Error handling works | `get_product_by_id` with ID -1 returns error string |
| Search works | `search_products` with `title=Generic` returns filtered results |
| Build clean | `dotnet build` returns 0 errors, 0 warnings |
