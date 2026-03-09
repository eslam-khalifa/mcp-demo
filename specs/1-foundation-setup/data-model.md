# Data Model: Foundation & Project Setup

**Feature**: 1-foundation-setup  
**Date**: 2026-03-09  
**Source**: [spec.md](spec.md) (FR-007 through FR-014)

---

## Entities

### Product

Represents a store item from the Platzi Fake Store API.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Id | integer | yes | Unique identifier assigned by the API |
| Title | string | yes | Product name / display title |
| Slug | string | yes | URL-friendly identifier |
| Price | decimal | yes | Product price (non-negative) |
| Description | string | yes | Detailed product description |
| Category | Category | yes | Associated category (navigation property) |
| Images | list of string | yes | URLs to product images (at least one) |
| CreationAt | datetime | yes | When the product was created (from API) |
| UpdatedAt | datetime | yes | When the product was last updated (from API) |

**Validation rules**:
- Price MUST be >= 0.
- Title MUST NOT be empty or whitespace.
- Images list MUST contain at least one URL.
- Category MUST NOT be null.

**Relationships**:
- Product → Category: Many-to-one (each product belongs to exactly one category).

---

### Category

Represents a product grouping from the Platzi Fake Store API.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Id | integer | yes | Unique identifier assigned by the API |
| Name | string | yes | Category display name |
| Slug | string | yes | URL-friendly identifier |
| Image | string | yes | URL to representative category image |
| CreationAt | datetime | yes | When the category was created (from API) |
| UpdatedAt | datetime | yes | When the category was last updated (from API) |

**Validation rules**:
- Name MUST NOT be empty or whitespace.
- Image MUST be a valid URL string.

**Relationships**:
- Category → Products: One-to-many (a category contains multiple products).
  Products are not stored on the category entity (fetched separately via API).

---

## Value Objects

### PriceRange

Defines a price window used for filtering operations.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| Min | decimal | yes | Minimum price (inclusive) |
| Max | decimal | yes | Maximum price (inclusive) |

**Validation rules**:
- Min MUST be >= 0.
- Max MUST be >= Min.
- `IsValid()` returns false if either rule is violated.

**Usage**: Used by `ISearchService.SearchProducts()` to express price
range filters. Also used internally for analytics queries.

**Immutability**: PriceRange is immutable (record type). Once created,
values cannot be changed.

---

## Shared Models

### Result<T>

A generic wrapper for operation outcomes.

| Attribute | Type | Description |
|-----------|------|-------------|
| IsSuccess | boolean | True if the operation succeeded |
| IsFailure | boolean | True if the operation failed (inverse of IsSuccess) |
| Value | T | The success payload (throws if accessed on failure) |
| Error | string | The error message (null/empty on success) |

**Factory methods**:
- `Success(T value)` — creates a success Result. Throws if value is null.
- `Failure(string error)` — creates a failure Result with error message.

**Constraints**:
- Success Result MUST carry a non-null payload.
- Accessing `Value` on a failed Result MUST throw an exception.
- Accessing `Error` on a success Result returns null or empty.

---

### ErrorResponse

A standardized error model for MCP tool error output.

| Attribute | Type | Description |
|-----------|------|-------------|
| ErrorType | string | Classification (e.g., "ValidationError", "ExternalApiError") |
| Message | string | Human-readable error description |
| ToolName | string (nullable) | Which MCP tool produced the error |
| Timestamp | datetime | When the error occurred (ISO 8601) |

---

## Exception Hierarchy

```text
System.Exception
├── DomainException                    # Domain layer base
│   └── EntityNotFoundException        # Specific: entity not found
├── McpToolException                   # Shared: MCP tool execution error
└── ExternalApiException               # Shared: external API error
```

| Exception | Layer | Attributes |
|-----------|-------|------------|
| DomainException | Domain | Message (inherited) |
| EntityNotFoundException | Domain | EntityType (string), EntityId (string) |
| McpToolException | Shared | ToolName (string), ErrorType (string) |
| ExternalApiException | Shared | StatusCode (int), ResponseBody (string) |

---

## Entity Relationship Diagram

```text
┌──────────────────┐          ┌──────────────────┐
│     Product      │          │    Category       │
├──────────────────┤          ├──────────────────┤
│ Id: int          │          │ Id: int           │
│ Title: string    │   N:1    │ Name: string      │
│ Slug: string     │─────────►│ Slug: string      │
│ Price: decimal   │          │ Image: string     │
│ Description: str │          │ CreationAt: dt    │
│ Images: string[] │          │ UpdatedAt: dt     │
│ CreationAt: dt   │          └──────────────────┘
│ UpdatedAt: dt    │
└──────────────────┘

┌──────────────────┐
│   PriceRange     │
├──────────────────┤
│ Min: decimal     │  (value object, no identity)
│ Max: decimal     │
│ IsValid(): bool  │
└──────────────────┘
```
