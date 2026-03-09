# Service Interface Contracts

**Feature**: 1-foundation-setup  
**Date**: 2026-03-09  
**Layer**: Application (MCPDemo.Application/Interfaces/)

All service methods return `Task<Result<T>>` where `Result<T>` is the
shared Result wrapper. Input parameters use primitives or DTOs.

---

## IProductService

**Location**: `src/MCPDemo.Application/Interfaces/IProductService.cs`  
**MCP Tool Category**: CRUD  
**Implementation Type**: C#

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| GetAllAsync | Result<IEnumerable<Product>> | int? offset, int? limit | List products with optional pagination |
| GetByIdAsync | Result<Product> | int id | Get single product by ID |
| GetBySlugAsync | Result<Product> | string slug | Get single product by slug |
| CreateAsync | Result<Product> | CreateProductDto dto | Create a new product |
| UpdateAsync | Result<Product> | int id, UpdateProductDto dto | Update an existing product |
| DeleteAsync | Result<bool> | int id | Delete a product |
| GetRelatedByIdAsync | Result<IEnumerable<Product>> | int id | Get related products by ID |
| GetRelatedBySlugAsync | Result<IEnumerable<Product>> | string slug | Get related products by slug |

---

## ICategoryService

**Location**: `src/MCPDemo.Application/Interfaces/ICategoryService.cs`  
**MCP Tool Category**: CRUD  
**Implementation Type**: C#

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| GetAllAsync | Result<IEnumerable<Category>> | — | List all categories |
| GetByIdAsync | Result<Category> | int id | Get single category by ID |
| GetBySlugAsync | Result<Category> | string slug | Get single category by slug |
| CreateAsync | Result<Category> | CreateCategoryDto dto | Create a new category |
| UpdateAsync | Result<Category> | int id, UpdateCategoryDto dto | Update an existing category |
| DeleteAsync | Result<bool> | int id | Delete a category |
| GetProductsAsync | Result<IEnumerable<Product>> | int categoryId | Get all products in a category |

---

## ISearchService

**Location**: `src/MCPDemo.Application/Interfaces/ISearchService.cs`  
**MCP Tool Category**: Search / Filter  
**Implementation Type**: C#

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| SearchProductsAsync | Result<IEnumerable<Product>> | SearchProductsDto filters | Search & filter products with combinable criteria |

**SearchProductsDto fields**: title?, price?, priceMin?, priceMax?,
categoryId?, categorySlug?, offset?, limit? — all optional.

---

## IAnalyticsService

**Location**: `src/MCPDemo.Application/Interfaces/IAnalyticsService.cs`  
**MCP Tool Category**: Reporting / Python Analysis  
**Implementation Type**: C# (orchestration) + Python (computation)

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| GetPriceStatisticsAsync | Result<PriceStatisticsDto> | int? categoryId | Min, max, mean, median, std dev of prices |
| GetTopExpensiveProductsAsync | Result<TopExpensiveDto> | int n, int? categoryId | Top N most expensive + average price |
| GetCategoryPriceReportAsync | Result<IEnumerable<CategoryReportDto>> | — | Price summary per category |
| GetPriceDistributionAsync | Result<PriceDistributionDto> | int? bucketCount | Price distribution histogram |

---

## IPythonSandboxService

**Location**: `src/MCPDemo.Application/Interfaces/IPythonSandboxService.cs`  
**MCP Tool Category**: Infrastructure abstraction  
**Implementation Type**: C# (Docker CLI invocation)

| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| ExecuteAsync | Result<string> | string toolName, string jsonInput | Execute a Python tool in Docker sandbox, return JSON output |

**Constraints** (from constitution):
- Docker container: `python:3.11-slim`
- Memory: 256 MB max
- CPU: 0.5 cores max
- Timeout: 30 seconds
- Network: none
- Filesystem: read-only
- Privileges: no-new-privileges
