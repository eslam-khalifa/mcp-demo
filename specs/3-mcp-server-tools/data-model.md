# Data Model: MCP Server & C# Tool Definitions

**Date**: 2026-03-10  
**Feature**: `3-mcp-server-tools`

## Overview

Phase 3 introduces no new domain entities. It operates on existing entities from Phase 1/2 and introduces **MCP tool definitions** as static method contracts. This document captures the tool-level data flow.

---

## Existing Entities (from Phase 1/2)

### Product
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary identifier |
| Title | string | Required |
| Slug | string | Auto-generated |
| Price | decimal | Must be >= 0 |
| Description | string | Required |
| Category | Category | Nested object |
| Images | List\<string\> | URLs |
| CreationAt | DateTime | ISO 8601 |
| UpdatedAt | DateTime | ISO 8601 |

### Category
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary identifier |
| Name | string | Required |
| Slug | string | Auto-generated |
| Image | string | URL |
| CreationAt | DateTime | ISO 8601 |
| UpdatedAt | DateTime | ISO 8601 |

---

## MCP Tool Definitions

### Tool Input/Output Mapping

Each MCP tool maps 1:1 to an Application-layer service method:

| MCP Tool Name | Service Method | Input Parameters | Output Type |
|---------------|----------------|------------------|-------------|
| `get_all_products` | `IProductService.GetAllAsync` | `offset?` (int), `limit?` (int) | `Product[]` JSON |
| `get_product_by_id` | `IProductService.GetByIdAsync` | `id` (int) | `Product` JSON |
| `get_product_by_slug` | `IProductService.GetBySlugAsync` | `slug` (string) | `Product` JSON |
| `create_product` | `IProductService.CreateAsync` | `title`, `price`, `description`, `categoryId`, `images` | `Product` JSON |
| `update_product` | `IProductService.UpdateAsync` | `id`, `title?`, `price?`, `description?`, `categoryId?`, `images?` | `Product` JSON |
| `delete_product` | `IProductService.DeleteAsync` | `id` (int) | `bool` JSON |
| `get_related_products_by_id` | `IProductService.GetRelatedByIdAsync` | `id` (int) | `Product[]` JSON |
| `get_related_products_by_slug` | `IProductService.GetRelatedBySlugAsync` | `slug` (string) | `Product[]` JSON |
| `get_all_categories` | `ICategoryService.GetAllAsync` | — | `Category[]` JSON |
| `get_category_by_id` | `ICategoryService.GetByIdAsync` | `id` (int) | `Category` JSON |
| `get_category_by_slug` | `ICategoryService.GetBySlugAsync` | `slug` (string) | `Category` JSON |
| `create_category` | `ICategoryService.CreateAsync` | `name`, `image` | `Category` JSON |
| `update_category` | `ICategoryService.UpdateAsync` | `id`, `name?`, `image?` | `Category` JSON |
| `delete_category` | `ICategoryService.DeleteAsync` | `id` (int) | `bool` JSON |
| `get_products_by_category` | `ICategoryService.GetProductsAsync` | `categoryId` (int) | `Product[]` JSON |
| `search_products` | `ISearchService.SearchProductsAsync` | `title?`, `price?`, `priceMin?`, `priceMax?`, `categoryId?`, `categorySlug?`, `offset?`, `limit?` | `Product[]` JSON |

### Data Flow

```
AI Assistant → JSON-RPC (stdio) → MCP SDK → [McpServerTool] method
    → Application Service → Result<T>
    → Tool unwraps: Success → JSON / Failure → plain string
    → JSON-RPC response → AI Assistant
```

---

## DI Registrations (Program.cs)

| Service Interface | Implementation | Lifetime |
|-------------------|----------------|----------|
| `IProductService` | `ProductService` | Scoped |
| `ICategoryService` | `CategoryService` | Scoped |
| `ISearchService` | `SearchService` | Scoped |
| `IPlatziStoreApiClient` | `PlatziStoreApiClient` | Scoped (via HttpClient) |
| `IMetricsCollector` | `InMemoryMetricsCollector` | Singleton |
