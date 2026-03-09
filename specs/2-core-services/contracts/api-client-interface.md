# Contract: IPlatziStoreApiClient

**Feature**: `2-core-services`  
**Date**: 2026-03-09  
**Layer**: Defined in Application, Implemented in Infrastructure

## Purpose

`IPlatziStoreApiClient` is the single point of access for all Platzi Fake
Store API communication. It encapsulates:

- HTTP request construction (URLs, query strings, request bodies)
- Retry logic (2 retries on 5xx/timeout, exponential backoff)
- Per-request timeout (15 seconds)
- Response deserialization (JSON → domain entities)
- Error mapping (HTTP errors → exceptions)

## Interface Contract

```
IPlatziStoreApiClient
├── Products
│   ├── GetAllProductsAsync(offset?, limit?) → IEnumerable<Product>
│   ├── GetProductByIdAsync(id) → Product
│   ├── GetProductBySlugAsync(slug) → Product
│   ├── CreateProductAsync(dto) → Product
│   ├── UpdateProductAsync(id, dto) → Product
│   ├── DeleteProductAsync(id) → bool
│   ├── GetRelatedProductsByIdAsync(id) → IEnumerable<Product>
│   └── GetRelatedProductsBySlugAsync(slug) → IEnumerable<Product>
├── Categories
│   ├── GetAllCategoriesAsync() → IEnumerable<Category>
│   ├── GetCategoryByIdAsync(id) → Category
│   ├── GetCategoryBySlugAsync(slug) → Category
│   ├── CreateCategoryAsync(dto) → Category
│   ├── UpdateCategoryAsync(id, dto) → Category
│   ├── DeleteCategoryAsync(id) → bool
│   └── GetProductsByCategoryAsync(categoryId) → IEnumerable<Product>
└── Search
    └── SearchProductsAsync(filters) → IEnumerable<Product>
```

## Error Behavior

| HTTP Status | Action | Exception Type |
|-------------|--------|---------------|
| 2xx | Return deserialized entity | None |
| 404 | Throw immediately (no retry) | `EntityNotFoundException` |
| 4xx (other) | Throw immediately (no retry) | `ExternalApiException` |
| 5xx | Retry up to 2 times, then throw | `ExternalApiException` |
| Timeout | Retry up to 2 times, then throw | `ExternalApiException` |

## Endpoint Mapping

| Method | HTTP | URL |
|--------|------|-----|
| `GetAllProductsAsync` | GET | `/products?offset={o}&limit={l}` |
| `GetProductByIdAsync` | GET | `/products/{id}` |
| `GetProductBySlugAsync` | GET | `/products/slug/{slug}` |
| `CreateProductAsync` | POST | `/products` |
| `UpdateProductAsync` | PUT | `/products/{id}` |
| `DeleteProductAsync` | DELETE | `/products/{id}` |
| `GetRelatedProductsByIdAsync` | GET | `/products/{id}/related` |
| `GetRelatedProductsBySlugAsync` | GET | `/products/slug/{slug}/related` |
| `GetAllCategoriesAsync` | GET | `/categories` |
| `GetCategoryByIdAsync` | GET | `/categories/{id}` |
| `GetCategoryBySlugAsync` | GET | `/categories/slug/{slug}` |
| `CreateCategoryAsync` | POST | `/categories` |
| `UpdateCategoryAsync` | PUT | `/categories/{id}` |
| `DeleteCategoryAsync` | DELETE | `/categories/{id}` |
| `GetProductsByCategoryAsync` | GET | `/categories/{id}/products` |
| `SearchProductsAsync` | GET | `/products?{query params}` |

## Search Query Parameter Construction

Given a `SearchProductsDto`, construct query string by including only
non-null parameters:

| DTO Field | Query Param |
|-----------|-------------|
| `Title` | `title` |
| `Price` | `price` |
| `PriceMin` | `price_min` |
| `PriceMax` | `price_max` |
| `CategoryId` | `categoryId` |
| `CategorySlug` | `categorySlug` |
| `Offset` | `offset` |
| `Limit` | `limit` |

## Consumers

- `ProductService` — uses product and related product methods
- `CategoryService` — uses category and products-by-category methods
- `SearchService` — uses search method
