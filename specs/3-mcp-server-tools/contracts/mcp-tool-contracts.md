# MCP Tool Contracts

**Date**: 2026-03-10  
**Feature**: `3-mcp-server-tools`  
**Transport**: stdio (JSON-RPC)

## Overview

This document defines the MCP tool contracts — the discoverable interface that the AI assistant sees when connecting to the MCP server. Each tool has a name, description, typed parameters, and return type.

---

## Product Tools

### get_all_products
**Description**: "List all products with optional pagination"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `offset` | int | No | Number of items to skip for pagination |
| `limit` | int | No | Maximum number of items to return |

**Returns**: JSON array of Product objects

---

### get_product_by_id
**Description**: "Get a single product by its unique ID"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The product ID |

**Returns**: JSON Product object, or error message if not found

---

### get_product_by_slug
**Description**: "Get a single product by its URL-friendly slug"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `slug` | string | Yes | The product slug |

**Returns**: JSON Product object, or error message if not found

---

### create_product
**Description**: "Create a new product in the store"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `title` | string | Yes | Product title |
| `price` | decimal | Yes | Product price (must be >= 0) |
| `description` | string | Yes | Product description |
| `categoryId` | int | Yes | Category ID the product belongs to |
| `images` | string[] | Yes | Array of image URLs |

**Returns**: JSON of the created Product object

---

### update_product
**Description**: "Update an existing product. Only provided fields will be changed."

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The product ID to update |
| `title` | string | No | New product title |
| `price` | decimal? | No | New product price |
| `description` | string | No | New product description |
| `categoryId` | int? | No | New category ID |
| `images` | string[] | No | New image URLs |

**Returns**: JSON of the updated Product object

---

### delete_product
**Description**: "Delete a product by its ID"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The product ID to delete |

**Returns**: `true` on success, or error message

---

### get_related_products_by_id
**Description**: "Get products related to a specific product by its ID"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The product ID to find related products for |

**Returns**: JSON array of related Product objects

---

### get_related_products_by_slug
**Description**: "Get products related to a specific product by its slug"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `slug` | string | Yes | The product slug to find related products for |

**Returns**: JSON array of related Product objects

---

## Category Tools

### get_all_categories
**Description**: "List all product categories"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| *(none)* | — | — | — |

**Returns**: JSON array of Category objects

---

### get_category_by_id
**Description**: "Get a single category by its unique ID"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The category ID |

**Returns**: JSON Category object, or error message if not found

---

### get_category_by_slug
**Description**: "Get a single category by its URL-friendly slug"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `slug` | string | Yes | The category slug |

**Returns**: JSON Category object, or error message if not found

---

### create_category
**Description**: "Create a new product category"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | Yes | Category name |
| `image` | string | Yes | Category image URL |

**Returns**: JSON of the created Category object

---

### update_category
**Description**: "Update an existing category. Only provided fields will be changed."

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The category ID to update |
| `name` | string | No | New category name |
| `image` | string | No | New category image URL |

**Returns**: JSON of the updated Category object

---

### delete_category
**Description**: "Delete a category by its ID"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | The category ID to delete |

**Returns**: `true` on success, or error message

---

### get_products_by_category
**Description**: "Get all products belonging to a specific category"

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `categoryId` | int | Yes | The category ID to list products for |

**Returns**: JSON array of Product objects in the category

---

## Search Tools

### search_products
**Description**: "Search and filter products using combinable criteria. All parameters are optional and can be combined."

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `title` | string | No | Filter by title (partial match) |
| `price` | decimal? | No | Filter by exact price |
| `priceMin` | decimal? | No | Minimum price (range filter) |
| `priceMax` | decimal? | No | Maximum price (range filter) |
| `categoryId` | int? | No | Filter by category ID |
| `categorySlug` | string | No | Filter by category slug |
| `offset` | int? | No | Number of items to skip |
| `limit` | int? | No | Maximum items to return |

**Returns**: JSON array of matching Product objects
