# MCP Backend Implementation Plan — Platzi Fake Store API Demo

**Version:** 1.0  
**Date:** 2026-03-09  
**Author:** Solom  
**Constitution:** [constitution.md](../\.specify/memory/constitution.md)

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure (Clean Architecture)](#3-project-structure-clean-architecture)
4. [Platzi Fake Store API — Complete Endpoint Reference](#4-platzi-fake-store-api--complete-endpoint-reference)
5. [Domain Entities](#5-domain-entities)
6. [MCP Tool Catalog](#6-mcp-tool-catalog)
7. [MCP Server Architecture (stdio)](#7-mcp-server-architecture-stdio)
8. [MCP Tool Execution Flow](#8-mcp-tool-execution-flow)
9. [Python Sandbox (Docker CLI)](#9-python-sandbox-docker-cli)
10. [Logging & Metrics (Serilog)](#10-logging--metrics-serilog)
11. [Error Handling Strategy](#11-error-handling-strategy)
12. [Testing Strategy](#12-testing-strategy)
13. [Security Considerations](#13-security-considerations)
14. [Implementation Roadmap](#14-implementation-roadmap)
15. [NuGet & Python Dependencies](#15-nuget--python-dependencies)

---

## 1. Project Overview

This project builds an **MCP (Model Context Protocol) server** that exposes
tools for interacting with the
[Platzi Fake Store API](https://api.escuelajs.co/api/v1/).
The MCP server connects to **Cursor IDE** via **stdio** transport, enabling
an AI assistant to browse, search, create, update, and delete products and
categories, as well as run advanced Python-based analytics in a Docker sandbox.

### Goals

- Expose every available Platzi Fake Store API operation as an MCP tool.
- Follow Clean Architecture with strict layer separation.
- Provide Python sandbox analytics for computations the API cannot do natively.
- Log every tool execution with Serilog (structured JSON to file).
- Collect runtime metrics (call counts, execution times, errors).
- Be modular, testable, and production-ready.

### Scope

- **In scope:** Products CRUD, Categories CRUD, filtering, pagination,
  related products, Python analytics, logging, metrics.
- **Out of scope (for now):** Users, Authentication, EF Core / database,
  response caching, frontend.

---

## 2. Technology Stack

| Layer | Technology |
|---|---|
| Language | C# 12, .NET 8 |
| Web Framework | ASP.NET Core (minimal — used only for DI container bootstrap) |
| MCP SDK | `ModelContextProtocol` NuGet (stdio transport) |
| HTTP Client | `HttpClient` via `IHttpClientFactory` |
| Logging | Serilog (JSON file sink) |
| Testing | xUnit, NSubstitute, FluentAssertions |
| Python Runtime | Python 3.11 in `python:3.11-slim` Docker container |
| Docker Invocation | `System.Diagnostics.Process` (`docker run` CLI) |
| Serialization | `System.Text.Json` |

---

## 3. Project Structure (Clean Architecture)

```
MCPDemo/
│
├── src/
│   ├── MCPDemo.Api/                    # API / MCP Host Layer
│   │   ├── Program.cs                  # MCP server bootstrap (stdio)
│   │   ├── McpTools/                   # MCP tool definitions (thin wrappers)
│   │   │   ├── ProductTools.cs         # Product MCP tool methods
│   │   │   ├── CategoryTools.cs        # Category MCP tool methods
│   │   │   ├── SearchTools.cs          # Search & filter MCP tool methods
│   │   │   └── AnalyticsTools.cs       # Python analytics MCP tool methods
│   │   └── Middleware/                 # Cross-cutting (error handler, logging)
│   │
│   ├── MCPDemo.Application/            # Application Layer
│   │   ├── Interfaces/                 # Service interfaces
│   │   │   ├── IProductService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   ├── ISearchService.cs
│   │   │   ├── IAnalyticsService.cs
│   │   │   └── IPythonSandboxService.cs
│   │   ├── Services/                   # Service implementations
│   │   │   ├── ProductService.cs
│   │   │   ├── CategoryService.cs
│   │   │   ├── SearchService.cs
│   │   │   └── AnalyticsService.cs
│   │   └── DTOs/                       # Request/Response DTOs
│   │       ├── Products/
│   │       └── Categories/
│   │
│   ├── MCPDemo.Domain/                 # Domain Layer
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   └── Category.cs
│   │   ├── ValueObjects/
│   │   │   └── PriceRange.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── EntityNotFoundException.cs
│   │
│   ├── MCPDemo.Infrastructure/         # Infrastructure Layer
│   │   ├── ExternalApi/                # Platzi Fake Store API client
│   │   │   ├── IPlatziStoreApiClient.cs
│   │   │   ├── PlatziStoreApiClient.cs
│   │   │   └── ApiModels/             # Raw API response models
│   │   ├── PythonSandbox/
│   │   │   ├── PythonSandboxService.cs
│   │   │   └── DockerProcessRunner.cs
│   │   ├── Logging/
│   │   │   └── SerilogConfiguration.cs
│   │   └── Metrics/
│   │       ├── IMetricsCollector.cs
│   │       └── InMemoryMetricsCollector.cs
│   │
│   └── MCPDemo.Shared/                 # Shared / Cross-Cutting
│       ├── Constants/
│       │   └── ApiConstants.cs
│       ├── Exceptions/
│       │   ├── McpToolException.cs
│       │   └── ExternalApiException.cs
│       ├── Extensions/
│       │   └── StringExtensions.cs
│       └── Models/
│           ├── Result.cs               # Generic Result<T> wrapper
│           └── ErrorResponse.cs
│
├── tests/
│   ├── MCPDemo.Application.Tests/      # Unit tests for services
│   ├── MCPDemo.Infrastructure.Tests/   # Unit tests for API client, sandbox
│   └── MCPDemo.Integration.Tests/      # End-to-end MCP tool tests
│
├── docker/
│   └── python-sandbox/
│       ├── Dockerfile
│       ├── requirements.txt
│       ├── main.py                     # Entry point for Python analytics
│       └── tools/                      # Individual Python tool scripts
│           ├── average_price.py
│           ├── price_statistics.py
│           └── category_report.py
│
├── logs/                               # Serilog output directory
├── MCPDemo.sln
└── docs/
    └── implementation-plan.md          # This file
```

### Layer Dependency Rules

```
MCPDemo.Api ──────────► MCPDemo.Application ──────► MCPDemo.Domain
     │                        │                           ▲
     │                        │                           │
     └──► MCPDemo.Infrastructure ─────────────────────────┘
                    │
                    └──► MCPDemo.Shared
                              ▲
                              │ (all layers may reference Shared)
```

- **Domain** has ZERO project references (pure business logic).
- **Application** references Domain only.
- **Infrastructure** references Domain + Shared.
- **Api** references Application + Infrastructure + Shared (for DI wiring).
- **Shared** has ZERO project references (cross-cutting utilities only).

---

## 4. Platzi Fake Store API — Complete Endpoint Reference

**Base URL:** `https://api.escuelajs.co/api/v1`

### 4.1 Products

| # | Method | Endpoint | Description | Query Params |
|---|--------|----------|-------------|--------------|
| P1 | `GET` | `/products` | List all products (50) | `offset`, `limit` |
| P2 | `GET` | `/products/{id}` | Get single product by ID | — |
| P3 | `GET` | `/products/slug/{slug}` | Get single product by slug | — |
| P4 | `POST` | `/products` | Create a product | — |
| P5 | `PUT` | `/products/{id}` | Update a product (partial) | — |
| P6 | `DELETE` | `/products/{id}` | Delete a product | — |
| P7 | `GET` | `/products/{id}/related` | Get related products by ID | — |
| P8 | `GET` | `/products/slug/{slug}/related` | Get related products by slug | — |

#### Product Create/Update Body

```json
{
  "title": "New Product",
  "price": 10,
  "description": "A description",
  "categoryId": 1,
  "images": ["https://placehold.co/600x400"]
}
```

> **Note:** For updates (`PUT`), only the fields being changed need to be sent.

#### Product Response Schema

```json
{
  "id": 1,
  "title": "Majestic Mountain Graphic T-Shirt",
  "slug": "majestic-mountain-graphic-t-shirt",
  "price": 44,
  "description": "Elevate your wardrobe...",
  "category": {
    "id": 1,
    "name": "Clothes",
    "slug": "clothes",
    "image": "https://i.imgur.com/QkIa5tT.jpeg",
    "creationAt": "2026-03-09T18:38:15.000Z",
    "updatedAt": "2026-03-09T18:38:15.000Z"
  },
  "images": [
    "https://i.imgur.com/QkIa5tT.jpeg",
    "https://i.imgur.com/jb5Yu0h.jpeg"
  ],
  "creationAt": "2026-03-09T18:38:15.000Z",
  "updatedAt": "2026-03-09T18:38:15.000Z"
}
```

### 4.2 Product Filtering

All filters are query parameters on `GET /products`:

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `title` | string | Filter by title (partial match) | `?title=Generic` |
| `price` | number | Filter by exact price | `?price=100` |
| `price_min` | number | Minimum price (range filter) | `?price_min=900` |
| `price_max` | number | Maximum price (range filter) | `?price_max=1000` |
| `categoryId` | number | Filter by category ID | `?categoryId=1` |
| `categorySlug` | string | Filter by category slug | `?categorySlug=clothes` |
| `offset` | number | Items to skip (pagination) | `?offset=0` |
| `limit` | number | Max items to return | `?limit=10` |

> **Key:** Filters are **combinable**. Example:
> `?title=Generic&price_min=900&price_max=1000&categoryId=1&limit=10&offset=0`

### 4.3 Categories

| # | Method | Endpoint | Description |
|---|--------|----------|-------------|
| C1 | `GET` | `/categories` | List all categories |
| C2 | `GET` | `/categories/{id}` | Get category by ID |
| C3 | `GET` | `/categories/slug/{slug}` | Get category by slug |
| C4 | `POST` | `/categories` | Create a category |
| C5 | `PUT` | `/categories/{id}` | Update a category |
| C6 | `DELETE` | `/categories/{id}` | Delete a category |
| C7 | `GET` | `/categories/{id}/products` | Get all products in category |

#### Category Create/Update Body

```json
{
  "name": "New Category",
  "image": "https://placeimg.com/640/480/any"
}
```

#### Category Response Schema

```json
{
  "id": 1,
  "name": "Clothes",
  "slug": "clothes",
  "image": "https://i.imgur.com/QkIa5tT.jpeg",
  "creationAt": "2026-03-09T18:38:15.000Z",
  "updatedAt": "2026-03-09T18:38:15.000Z"
}
```

### 4.4 Available Categories (at time of writing)

| ID | Name | Slug |
|----|------|------|
| 1 | Clothes | clothes |
| 2 | Electronics | electronics |
| 3 | Furniture | furniture |
| 4 | Shoes | shoes |
| 5 | Miscellaneous | miscellaneous |

---

## 5. Domain Entities

### Product

```csharp
public class Product
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public List<string> Images { get; set; }
    public DateTime CreationAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Category

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Image { get; set; }
    public DateTime CreationAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### PriceRange (Value Object)

```csharp
public record PriceRange(decimal Min, decimal Max)
{
    public bool IsValid() => Min >= 0 && Max >= Min;
}
```

---

## 6. MCP Tool Catalog

Every tool below becomes an MCP tool method exposed via the stdio server.

### 6.1 Product CRUD Tools (C#)

| Tool Name | Category | Description | Inputs | Outputs |
|-----------|----------|-------------|--------|---------|
| `get_all_products` | CRUD | List all products with optional pagination | `offset?`, `limit?` | `Product[]` |
| `get_product_by_id` | CRUD | Get a single product by ID | `id` (int) | `Product` |
| `get_product_by_slug` | CRUD | Get a single product by slug | `slug` (string) | `Product` |
| `create_product` | CRUD | Create a new product | `title`, `price`, `description`, `categoryId`, `images[]` | `Product` |
| `update_product` | CRUD | Update an existing product | `id`, `title?`, `price?`, `description?`, `categoryId?`, `images[]?` | `Product` |
| `delete_product` | CRUD | Delete a product by ID | `id` (int) | `bool` |
| `get_related_products_by_id` | CRUD | Get products related to a product (by ID) | `id` (int) | `Product[]` |
| `get_related_products_by_slug` | CRUD | Get products related to a product (by slug) | `slug` (string) | `Product[]` |

### 6.2 Category CRUD Tools (C#)

| Tool Name | Category | Description | Inputs | Outputs |
|-----------|----------|-------------|--------|---------|
| `get_all_categories` | CRUD | List all categories | — | `Category[]` |
| `get_category_by_id` | CRUD | Get a single category by ID | `id` (int) | `Category` |
| `get_category_by_slug` | CRUD | Get a single category by slug | `slug` (string) | `Category` |
| `create_category` | CRUD | Create a new category | `name`, `image` (URL) | `Category` |
| `update_category` | CRUD | Update an existing category | `id`, `name?`, `image?` | `Category` |
| `delete_category` | CRUD | Delete a category by ID | `id` (int) | `bool` |
| `get_products_by_category` | CRUD | Get all products in a category | `categoryId` (int) | `Product[]` |

### 6.3 Search & Filter Tools (C#)

| Tool Name | Category | Description | Inputs | Outputs |
|-----------|----------|-------------|--------|---------|
| `search_products` | Search | Search and filter products with combinable criteria | `title?`, `price?`, `price_min?`, `price_max?`, `categoryId?`, `categorySlug?`, `offset?`, `limit?` | `Product[]` |

> **Design decision:** A single `search_products` tool with all optional
> filter parameters is more flexible than many small filter tools. The AI
> can combine any filters naturally in a single call.

### 6.4 Python Analytics Tools

| Tool Name | Category | Description | Inputs | Outputs |
|-----------|----------|-------------|--------|---------|
| `analyze_price_statistics` | Reporting | Compute min, max, mean, median, std dev of prices | `categoryId?` (optional filter) | `{ min, max, mean, median, stdDev, count }` |
| `get_top_expensive_products` | Reporting | Get top N most expensive products and their average price | `n` (int), `categoryId?` | `{ products[], averagePrice }` |
| `get_category_price_report` | Reporting | Price summary (avg, min, max, count) per category | — | `{ categoryReports[] }` |
| `get_price_distribution` | Reporting | Price distribution (histogram buckets) | `bucketCount?` (default: 10) | `{ buckets[] }` |

---

## 7. MCP Server Architecture (stdio)

### Bootstrap (`Program.cs`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;

var builder = Host.CreateApplicationBuilder(args);

// Register services (Application layer)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register infrastructure
builder.Services.AddHttpClient<IPlatziStoreApiClient, PlatziStoreApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.escuelajs.co/api/v1/");
});
builder.Services.AddScoped<IPythonSandboxService, PythonSandboxService>();
builder.Services.AddSingleton<IMetricsCollector, InMemoryMetricsCollector>();

// Register Serilog
builder.Services.AddSerilog(config =>
    config.WriteTo.File("logs/mcp-tool-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:o} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

// Register MCP server with stdio transport
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
```

### MCP Tool Definition Pattern

Each tool is a static method decorated with `[McpServerToolType]` and
`[McpServerTool]` attributes:

```csharp
[McpServerToolType]
public static class ProductTools
{
    [McpServerTool, Description("Get a single product by its ID")]
    public static async Task<string> GetProductById(
        IProductService productService,
        [Description("The product ID")] int id)
    {
        var result = await productService.GetByIdAsync(id);
        return JsonSerializer.Serialize(result);
    }
}
```

---

## 8. MCP Tool Execution Flow

```
┌─────────┐     stdio      ┌──────────────┐
│  Cursor  │ ◄────────────► │  MCP Server  │
│   IDE    │   JSON-RPC     │  (Program.cs)│
└─────────┘                 └──────┬───────┘
                                   │
                            ┌──────▼───────┐
                            │  MCP Tool    │  (McpTools/*.cs)
                            │  Definition  │
                            └──────┬───────┘
                                   │
                            ┌──────▼───────┐
                            │  Application │  (Services/*.cs)
                            │  Service     │  - Input validation
                            └──────┬───────┘  - Orchestration
                                   │
                    ┌──────────────┼──────────────┐
                    ▼              ▼               ▼
            ┌──────────┐   ┌──────────┐   ┌──────────────┐
            │ Platzi   │   │ Python   │   │ Metrics      │
            │ API      │   │ Sandbox  │   │ Collector    │
            │ Client   │   │ (Docker) │   │              │
            └──────────┘   └──────────┘   └──────────────┘
                                │
                         ┌──────▼───────┐
                         │ docker run   │
                         │ python:3.11  │
                         │ --rm --net=  │
                         │ JSON stdin → │
                         │ JSON stdout  │
                         └──────────────┘
```

### Execution Steps (C# Tool)

1. Cursor sends JSON-RPC tool call via stdin.
2. MCP SDK deserializes and routes to the matching `[McpServerTool]` method.
3. Tool method calls the Application Service.
4. Service validates input, calls `PlatziStoreApiClient`.
5. API client makes HTTP call with retry logic (max 2 retries).
6. Response is mapped to domain entity → DTO → JSON string.
7. Metrics are recorded (execution time, success/failure).
8. Serilog logs the execution.
9. Result returned to Cursor via stdout.

### Execution Steps (Python Tool)

1. Steps 1–3 same as above.
2. Service fetches required data from Platzi API.
3. Service serializes data to JSON and calls `PythonSandboxService`.
4. `PythonSandboxService` runs `docker run` with JSON piped to stdin.
5. Python script processes data and writes JSON to stdout.
6. Service deserializes Python output and returns result.
7. Metrics + logging as above.

---

## 9. Python Sandbox (Docker CLI)

### Dockerfile (`docker/python-sandbox/Dockerfile`)

```dockerfile
FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

ENTRYPOINT ["python", "main.py"]
```

### `requirements.txt`

```
pandas==2.2.0
numpy==1.26.3
```

> **No `requests` library** — Python tools receive all data via stdin JSON.
> They do NOT make network calls. This satisfies the constitution's
> "no network access outside MCP backend API" rule.

### `main.py` — Entry Point

```python
import sys
import json

def main():
    input_data = json.loads(sys.stdin.read())
    tool_name = input_data.get("tool")
    payload = input_data.get("data")

    if tool_name == "price_statistics":
        from tools.price_statistics import run
    elif tool_name == "top_expensive":
        from tools.average_price import run
    elif tool_name == "category_report":
        from tools.category_report import run
    elif tool_name == "price_distribution":
        from tools.price_distribution import run
    else:
        print(json.dumps({"error": f"Unknown tool: {tool_name}"}))
        sys.exit(1)

    result = run(payload)
    print(json.dumps(result))

if __name__ == "__main__":
    main()
```

### Docker Invocation from C#

```csharp
public class PythonSandboxService : IPythonSandboxService
{
    public async Task<string> ExecuteAsync(string toolName, string jsonInput)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "run --rm -i --memory=256m --cpus=0.5 " +
                        "--network=none --read-only " +
                        "--security-opt=no-new-privileges " +
                        "mcp-python-sandbox",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        // Write JSON input to stdin
        await process.StandardInput.WriteAsync(jsonInput);
        process.StandardInput.Close();

        // Apply 30-second timeout
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var output = await process.StandardOutput.ReadToEndAsync(cts.Token);

        await process.WaitForExitAsync(cts.Token);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new PythonSandboxException(
                $"Python tool '{toolName}' failed: {error}");
        }

        return output;
    }
}
```

### Resource Limits (enforced by Docker flags)

| Resource | Limit | Docker Flag |
|----------|-------|-------------|
| Memory | 256 MB | `--memory=256m` |
| CPU | 0.5 cores | `--cpus=0.5` |
| Timeout | 30 seconds | C# `CancellationTokenSource` |
| Network | None | `--network=none` |
| Filesystem | Read-only | `--read-only` |
| Privileges | No escalation | `--security-opt=no-new-privileges` |

---

## 10. Logging & Metrics (Serilog)

### Serilog Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "logs/mcp-tool-.log",
        rollingInterval: RollingInterval.Day,
        formatter: new JsonFormatter())
    .CreateLogger();
```

### What Gets Logged (per tool execution)

| Field | Description | Format |
|-------|-------------|--------|
| `Timestamp` | When the tool was invoked | ISO 8601 |
| `ToolName` | MCP tool name | string |
| `Inputs` | Serialized input parameters | JSON |
| `Output` | Serialized output (summary) | JSON (truncated if large) |
| `ExecutionTimeMs` | Wall-clock execution time | number (ms) |
| `Success` | Whether execution succeeded | bool |
| `ErrorType` | Error classification (if failed) | string |
| `ErrorMessage` | Error detail (if failed) | string |

> **Security:** Logs MUST NOT contain API keys, tokens, or PII.

### Metrics Collector (In-Memory)

```csharp
public interface IMetricsCollector
{
    void RecordExecution(string toolName, long elapsedMs, bool success,
                         string? errorType = null);
    ToolMetrics GetMetrics(string toolName);
    IReadOnlyDictionary<string, ToolMetrics> GetAllMetrics();
}

public class ToolMetrics
{
    public int TotalCalls { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public int PythonSandboxExecutions { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; }
}
```

Metrics are exposed via an optional MCP tool (`get_metrics`) so the AI can
query runtime health.

---

## 11. Error Handling Strategy

### Error Categories

| Error Type | Source | Handling |
|------------|--------|----------|
| `ValidationError` | Input validation in Application layer | Return descriptive message, no retry |
| `ExternalApiError` | Platzi API returns 4xx/5xx | Retry up to 2 times for 5xx; return error for 4xx |
| `EntityNotFoundError` | Platzi API returns 404 | Return "not found" message, no retry |
| `PythonSandboxError` | Docker execution fails | Log full error, return sanitized message |
| `TimeoutError` | Python sandbox exceeds 30s | Kill process, return timeout error |
| `UnexpectedError` | Any unhandled exception | Log stack trace, return generic error |

### Retry Policy (for Platzi API calls)

```csharp
// In PlatziStoreApiClient — simplified
private async Task<HttpResponseMessage> SendWithRetryAsync(
    HttpRequestMessage request, int maxRetries = 2)
{
    int attempt = 0;
    while (true)
    {
        attempt++;
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode || attempt > maxRetries
            || (int)response.StatusCode < 500)
        {
            return response;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt));
    }
}
```

### Error Response Model

```csharp
public class ErrorResponse
{
    public string ErrorType { get; set; }      // e.g., "ValidationError"
    public string Message { get; set; }         // Human-readable description
    public string? ToolName { get; set; }       // Which tool failed
    public DateTime Timestamp { get; set; }     // ISO 8601
}
```

---

## 12. Testing Strategy

### Framework & Libraries

| Library | Purpose |
|---------|---------|
| **xUnit** | Test runner and assertions |
| **NSubstitute** | Mocking interfaces |
| **FluentAssertions** | Expressive assertion syntax (optional but recommended) |

### Test Categories

#### Unit Tests (`MCPDemo.Application.Tests`)

- Test each service method in isolation.
- Mock `IPlatziStoreApiClient` with NSubstitute.
- Mock `IPythonSandboxService` for analytics tests.
- Verify input validation (invalid ID, empty title, negative price, etc.).
- Verify correct mapping from API models to domain entities.

#### Unit Tests (`MCPDemo.Infrastructure.Tests`)

- Test `PlatziStoreApiClient` with mocked `HttpMessageHandler`.
- Test retry logic (simulate 500 → 500 → 200).
- Test `PythonSandboxService` with mocked `Process` (verify arguments,
  timeout handling).
- Test `InMemoryMetricsCollector` thread safety and accuracy.

#### Integration Tests (`MCPDemo.Integration.Tests`)

- Test full MCP tool flow with actual Platzi API (network-dependent,
  marked with `[Trait("Category", "Integration")]`).
- Verify JSON serialization round-trip.
- Verify Docker sandbox with a simple Python script (requires Docker).

### Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior
```

Example: `GetByIdAsync_ValidId_ReturnsProduct`

---

## 13. Security Considerations

| Concern | Mitigation |
|---------|------------|
| Untrusted Python code | Docker sandbox with `--network=none`, `--read-only`, `--security-opt=no-new-privileges` |
| Input injection | All inputs validated in Application layer before passing to infrastructure |
| Secret exposure in logs | Serilog configuration excludes sensitive fields; no API keys stored |
| Resource abuse | Docker enforces memory (256MB), CPU (0.5), and timeout (30s) limits |
| External API failures | Retry + structured error propagation; no raw exception bubbling |

---

## 14. Implementation Roadmap

| Phase | Step | Task | Layer | Est. Effort |
|-------|------|------|-------|-------------|
| **1** | 1 | Create solution + 5 project structure | All | 1h |
| **1** | 2 | Add NuGet dependencies to all projects | All | 30m |
| **1** | 3 | Define domain entities (`Product`, `Category`, `PriceRange`) | Domain | 30m |
| **1** | 4 | Create shared models (`Result<T>`, `ErrorResponse`, constants) | Shared | 30m |
| **1** | 5 | Create all service interfaces in Application | Application | 30m |
| **2** | 6 | Implement `PlatziStoreApiClient` with retry logic | Infrastructure | 2h |
| **2** | 7 | Implement `ProductService` | Application | 1.5h |
| **2** | 8 | Implement `CategoryService` | Application | 1h |
| **2** | 9 | Implement `SearchService` (filter/pagination) | Application | 1h |
| **2** | 10 | Configure Serilog + implement `InMemoryMetricsCollector` | Infrastructure | 1h |
| **3** | 11 | Bootstrap MCP server (`Program.cs` with stdio) | Api | 1h |
| **3** | 12 | Implement Product MCP tools (`ProductTools.cs`) | Api | 1.5h |
| **3** | 13 | Implement Category MCP tools (`CategoryTools.cs`) | Api | 1h |
| **3** | 14 | Implement Search MCP tool (`SearchTools.cs`) | Api | 1h |
| **4** | 15 | Create Python sandbox Dockerfile + `main.py` | Docker | 1h |
| **4** | 16 | Implement Python analytics scripts | Docker | 2h |
| **4** | 17 | Implement `PythonSandboxService` (Docker CLI runner) | Infrastructure | 1.5h |
| **4** | 18 | Implement `AnalyticsService` | Application | 1h |
| **4** | 19 | Implement Analytics MCP tools (`AnalyticsTools.cs`) | Api | 1h |
| **5** | 20 | Write unit tests for Application services | Tests | 3h |
| **5** | 21 | Write unit tests for Infrastructure (API client, sandbox) | Tests | 2h |
| **5** | 22 | Write integration tests (MCP tool end-to-end) | Tests | 2h |
| **6** | 23 | Add `get_metrics` MCP tool | Api | 30m |
| **6** | 24 | Final code review, cleanup, documentation | All | 1h |

### Phase Summary

| Phase | Name | Duration |
|-------|------|----------|
| 1 | Foundation & Setup | ~3h |
| 2 | Core Services & Infrastructure | ~6.5h |
| 3 | MCP Server & C# Tools | ~4.5h |
| 4 | Python Sandbox & Analytics | ~5.5h |
| 5 | Testing | ~7h |
| 6 | Polish & Metrics | ~1.5h |
| | **Total** | **~28h** |

---

## 15. NuGet & Python Dependencies

### NuGet Packages

| Package | Project | Purpose |
|---------|---------|---------|
| `ModelContextProtocol` | MCPDemo.Api | MCP server SDK (stdio) |
| `Microsoft.Extensions.Hosting` | MCPDemo.Api | Host builder + DI |
| `Microsoft.Extensions.Http` | MCPDemo.Infrastructure | `IHttpClientFactory` |
| `Serilog.AspNetCore` | MCPDemo.Api | Serilog integration |
| `Serilog.Sinks.File` | MCPDemo.Infrastructure | File sink |
| `Serilog.Formatting.Compact` | MCPDemo.Infrastructure | JSON formatter |
| `System.Text.Json` | MCPDemo.Shared | JSON serialization |
| `xunit` | Tests | Test framework |
| `xunit.runner.visualstudio` | Tests | VS test integration |
| `NSubstitute` | Tests | Mocking |
| `FluentAssertions` | Tests | Readable assertions |
| `Microsoft.NET.Test.Sdk` | Tests | Test SDK |

### Python Packages (`requirements.txt`)

| Package | Version | Purpose |
|---------|---------|---------|
| `pandas` | 2.2.0 | Data analysis (aggregation, filtering) |
| `numpy` | 1.26.3 | Numerical computation (statistics) |

---

## Appendix: Constitution Compliance Checklist

| Constitution Principle | Compliance |
|------------------------|------------|
| I. MCP Tool Standards (definition contract) | ✅ Every tool has name, description, inputs, outputs, category, impl type |
| II. API Calling & Efficiency (retry, error handling) | ✅ Retry max 2, structured error model, no raw exceptions |
| III. Logging & Metrics (file logging, 4 counters) | ✅ Serilog JSON file, InMemoryMetricsCollector with all 4 metrics |
| IV. Clean Architecture (4 layers + Shared) | ✅ Strict dependency direction, MCP tools in Application layer |
| V. Security & Sandboxing (Docker, validation) | ✅ Docker with no-network, read-only, no-new-privileges, input validation |
| Technology Stack (.NET 8, C#, Serilog) | ✅ All technologies as specified |
| Spec-Kit Conventions (per-tool spec files) | ⏳ To be created per tool during implementation |
| No caching | ✅ No cache layer included |
| No EF Core / database | ✅ No database, all data from external API |
