# Feature Specification: Core Services & Infrastructure

**Feature Branch**: `2-core-services`  
**Created**: 2026-03-09  
**Status**: Draft  
**Input**: User description: "Phase 2 from the implementation plan — implement PlatziStoreApiClient with retry logic, ProductService, CategoryService, SearchService, configure Serilog logging, and implement InMemoryMetricsCollector"

## Clarifications

### Session 2026-03-09

- Q: What happens when a product/category update sends an empty update DTO (no fields changed)? → A: Reject with a validation error — return a failure Result with "No fields to update" message without calling the external API.
- Q: What is the per-request HTTP timeout for individual API calls? → A: 15 seconds per request. With 2 retries and exponential backoff, worst-case total wait is approximately 47 seconds.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Product Data Available via API Client (Priority: P1)

A downstream service (e.g., ProductService) calls the API client to retrieve,
create, update, or delete products from the Platzi Fake Store. The client
handles HTTP communication, automatic retries on server errors, and maps
raw API responses to domain entities. When the external API is temporarily
unavailable (5xx), the client retries up to 2 times before reporting failure.

**Why this priority**: The API client is the foundation of all data access.
Without it, no service can retrieve or modify store data. Every other user
story depends on this.

**Independent Test**: Can be tested by calling each API client method against
a mocked HTTP handler, verifying correct URL construction, request bodies,
retry behavior, and response mapping.

**Acceptance Scenarios**:

1. **Given** a valid product ID, **When** the API client requests the product,
   **Then** it returns a fully populated Product domain entity with all
   attributes mapped correctly.
2. **Given** the external API returns a 500 error on the first attempt,
   **When** the API client makes a request, **Then** it automatically retries
   up to 2 additional times with increasing delay before reporting failure.
3. **Given** the external API returns a 404, **When** the API client receives
   the response, **Then** it does NOT retry and immediately returns a failure
   result.
4. **Given** a valid create-product request body, **When** the API client
   sends the request, **Then** the new product is returned with an assigned
   ID and all provided attributes.
5. **Given** a product ID and partial update data, **When** the API client
   sends a PUT request, **Then** only the provided fields are updated and
   the updated product is returned.
6. **Given** the external API does not respond within 15 seconds, **When**
   the API client times out, **Then** it treats the timeout as a retriable
   error and retries according to the retry policy.

---

### User Story 2 - Product Operations Orchestrated Through Service (Priority: P2)

A caller (e.g., an MCP tool) uses ProductService to perform product
operations. The service validates inputs, delegates to the API client, and
wraps responses in the Result<T> pattern. Invalid inputs (negative prices,
empty titles) are caught before any external call is made.

**Why this priority**: ProductService is the primary consumer of the API
client and represents the most-used functionality in the system.

**Independent Test**: Can be tested by mocking the API client and verifying
that ProductService correctly validates inputs, delegates calls, and wraps
results.

**Acceptance Scenarios**:

1. **Given** a caller requests all products with pagination, **When**
   ProductService processes the request, **Then** it returns a success
   Result containing the product list.
2. **Given** a caller provides a negative price for product creation,
   **When** ProductService validates the input, **Then** it returns a
   failure Result with a descriptive validation error without calling the
   external API.
3. **Given** the API client returns a failure, **When** ProductService
   processes the response, **Then** it wraps the error in a failure Result
   with a clear error message.
4. **Given** a valid product ID, **When** ProductService requests related
   products, **Then** it returns a success Result containing the related
   product list.

---

### User Story 3 - Category Operations Orchestrated Through Service (Priority: P3)

A caller uses CategoryService to manage categories and retrieve products
by category. The service validates inputs and delegates to the API client,
following the same patterns established by ProductService.

**Why this priority**: Category operations complement product operations
and enable organizing products. They follow the same service pattern,
making them straightforward to implement after ProductService.

**Independent Test**: Can be tested by mocking the API client and verifying
CategoryService correctly handles all CRUD operations and products-by-category
queries.

**Acceptance Scenarios**:

1. **Given** a caller requests all categories, **When** CategoryService
   processes the request, **Then** it returns a success Result containing
   the category list.
2. **Given** a valid category ID, **When** CategoryService requests products
   in that category, **Then** it returns a success Result containing the
   product list.
3. **Given** a caller provides an empty name for category creation, **When**
   CategoryService validates the input, **Then** it returns a failure Result
   with a descriptive validation error.

---

### User Story 4 - Product Search with Combined Filters (Priority: P4)

A caller uses SearchService to find products matching combined filter
criteria (title, price range, category, pagination). The service constructs
the correct query parameters and delegates to the API client.

**Why this priority**: Search is a key use case for the MCP tools — the AI
assistant needs to find specific products efficiently. However, it depends
on the API client being fully functional first.

**Independent Test**: Can be tested by mocking the API client and verifying
correct query parameter construction for various filter combinations.

**Acceptance Scenarios**:

1. **Given** a caller provides a title filter, **When** SearchService
   builds the query, **Then** the correct query parameters are sent to the
   API client and matching products are returned.
2. **Given** a caller provides price range and category filters, **When**
   SearchService builds the query, **Then** all filters are combined into a
   single query string.
3. **Given** a caller provides no filters, **When** SearchService processes
   the request, **Then** all products are returned (same as get-all with
   pagination).

---

### User Story 5 - Tool Executions Logged and Metered (Priority: P5)

Every service call is logged with structured data (tool name, inputs, output
summary, execution time, success/failure) and runtime metrics are collected
in memory (call counts, success rates, average execution times, error
breakdowns).

**Why this priority**: Logging and metrics are cross-cutting concerns that
enhance debuggability and runtime visibility. They are not required for core
functionality but add significant operational value.

**Independent Test**: Can be tested by invoking a service method and
verifying that the logger captured the expected structured data and the
metrics collector updated its counters correctly.

**Acceptance Scenarios**:

1. **Given** a service method is invoked, **When** it completes successfully,
   **Then** a structured log entry is written containing tool name, inputs,
   execution time, and success status.
2. **Given** a service method fails, **When** the error is caught, **Then**
   a structured log entry is written containing error type and error message
   in addition to the standard fields.
3. **Given** multiple tool executions occur, **When** a caller queries the
   metrics collector, **Then** it returns accurate aggregated data (total
   calls, success count, failure count, average execution time) per tool.
4. **Given** concurrent tool executions, **When** the metrics collector is
   updated simultaneously, **Then** all updates are recorded without data
   loss or corruption.

---

### Edge Cases

- What happens when the external API returns an unexpected response format
  (e.g., missing fields, extra fields)? The API client should handle
  partial responses gracefully, using defaults for missing optional fields
  and ignoring unexpected extra fields.
- What happens when the external API is completely unreachable (network
  timeout)? The API client should treat this as a retriable error and
  attempt retries, then surface a clear ExternalApiException.
- What happens when retry delays exceed a reasonable total wait time? The
  retry strategy uses exponential backoff capped at 2 retries (500ms,
  1000ms), so maximum additional wait is 1.5 seconds.
- What happens when the metrics collector is queried for a tool that has
  never been executed? It should return a ToolMetrics object with all
  counters at zero.
- What happens when a product or category update sends an empty update DTO
  (no fields changed)? The service MUST reject the request and return a
  failure Result with a "No fields to update" validation error without
  calling the external API.
- What happens when an individual HTTP request takes longer than expected?
  Each request has a 15-second timeout. Timeouts are treated as retriable
  errors. With 2 retries and backoff delays (500ms, 1000ms), the worst-case
  total wait is approximately 47 seconds.

## Requirements *(mandatory)*

### Functional Requirements

#### API Client

- **FR-001**: System MUST provide an API client that communicates with all
  Platzi Fake Store product endpoints (list, get by ID, get by slug, create,
  update, delete, related by ID, related by slug).
- **FR-002**: System MUST provide an API client that communicates with all
  Platzi Fake Store category endpoints (list, get by ID, get by slug, create,
  update, delete, products by category).
- **FR-003**: The API client MUST automatically retry requests that receive
  5xx server errors, up to 2 additional retries with exponential backoff
  (500ms × attempt number).
- **FR-004**: The API client MUST NOT retry requests that receive 4xx client
  errors (including 404).
- **FR-005**: The API client MUST map raw API JSON responses to domain
  entities (Product, Category) using the project's serialization conventions.
- **FR-006**: The API client MUST construct proper query strings for product
  filtering and pagination parameters.
- **FR-007**: The API client MUST use a centralized HTTP client obtained via
  dependency injection (not creating new HttpClient instances directly).
- **FR-023**: The API client MUST enforce a 15-second timeout on each
  individual HTTP request. Timeouts MUST be treated as retriable errors
  subject to the same retry policy as 5xx responses.

#### Product Service

- **FR-008**: System MUST provide a ProductService that exposes all product
  operations (list, get by ID, get by slug, create, update, delete, related
  by ID, related by slug) through the IProductService interface.
- **FR-009**: ProductService MUST validate input data before calling the API
  client — rejecting empty titles, negative prices, missing required fields,
  and empty update DTOs (no fields changed) with descriptive error messages.
- **FR-010**: ProductService MUST wrap all responses in the Result<T> pattern,
  returning success with payload or failure with error message.

#### Category Service

- **FR-011**: System MUST provide a CategoryService that exposes all category
  operations (list, get by ID, get by slug, create, update, delete, products
  by category) through the ICategoryService interface.
- **FR-012**: CategoryService MUST validate input data before calling the API
  client — rejecting empty names, missing required fields, and empty update
  DTOs (no fields changed).
- **FR-013**: CategoryService MUST wrap all responses in the Result<T> pattern.

#### Search Service

- **FR-014**: System MUST provide a SearchService that accepts combinable
  filter criteria (title, price, price range, category ID, category slug,
  offset, limit) and returns matching products.
- **FR-015**: SearchService MUST construct correct query parameters from the
  filter DTO, omitting parameters that are null.
- **FR-016**: SearchService MUST wrap all responses in the Result<T> pattern.

#### Logging

- **FR-017**: System MUST configure structured logging that writes JSON
  log entries to rolling log files (one file per day).
- **FR-018**: Every service method execution MUST be logged with: tool name,
  input parameters, execution time in milliseconds, success/failure status,
  and error details if applicable.
- **FR-019**: Log entries MUST NOT contain sensitive data (API keys, tokens,
  or personally identifiable information).

#### Metrics

- **FR-020**: System MUST provide an in-memory metrics collector that tracks
  per-tool statistics: total call count, success count, failure count,
  average execution time in milliseconds, and error count by error type.
- **FR-021**: The metrics collector MUST be thread-safe, correctly handling
  concurrent updates from multiple service calls.
- **FR-022**: The metrics collector MUST expose methods to retrieve metrics
  for a single tool and for all tools.

### Key Entities

- **PlatziStoreApiClient**: The HTTP client that communicates with the
  external Platzi Fake Store API. Handles request construction, retry
  logic, response deserialization, and error mapping.
- **ProductService**: Application-layer orchestrator for product operations.
  Validates inputs, delegates to the API client, and wraps responses.
- **CategoryService**: Application-layer orchestrator for category operations.
  Follows the same pattern as ProductService.
- **SearchService**: Application-layer service for product search with
  combinable filters. Constructs query parameters and delegates to the
  API client.
- **InMemoryMetricsCollector**: Thread-safe in-memory store for runtime
  metrics. Tracks per-tool execution statistics.
- **ToolMetrics**: Data model representing aggregated metrics for a single
  tool (counts, averages, error breakdown).

### Assumptions

- The Platzi Fake Store API is available at `https://api.escuelajs.co/api/v1`
  and does not require authentication.
- The API returns JSON responses matching the documented schemas (Product
  and Category response formats).
- No response caching is used — every request goes directly to the external
  API (per the project's "no caching" rule).
- The API client uses `System.Text.Json` for serialization with camelCase
  property naming.
- Serilog is configured in the Infrastructure layer and wired through
  dependency injection.
- The metrics collector is registered as a singleton to accumulate data
  across the application lifetime.
- NuGet packages for this phase: `Microsoft.Extensions.Http`,
  `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Serilog.Formatting.Compact`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All product operations (8 methods) return correct results when
  given valid inputs, matching the expected Platzi Fake Store API behavior.
- **SC-002**: All category operations (7 methods) return correct results when
  given valid inputs, matching the expected API behavior.
- **SC-003**: The API client correctly retries on 5xx errors (max 2 retries)
  and does NOT retry on 4xx errors, with 100% accuracy across all test
  scenarios.
- **SC-004**: Input validation catches all invalid inputs (empty titles,
  negative prices, missing required fields, empty update DTOs) and returns
  descriptive failure Results without calling the external API.
- **SC-005**: Product search correctly combines any combination of filter
  parameters into valid query strings, verified across at least 5 different
  filter combinations.
- **SC-006**: Every service method execution produces a structured log entry
  containing all required fields (tool name, inputs, execution time,
  success/failure).
- **SC-007**: The metrics collector accurately tracks call counts, success
  rates, and average execution times across at least 100 simulated
  concurrent operations with zero data loss.
- **SC-008**: The entire solution builds with zero errors and zero warnings
  after adding all Phase 2 code and NuGet dependencies.
- **SC-009**: Individual HTTP requests that exceed 15 seconds are timed out
  and retried, with the total worst-case wait not exceeding 50 seconds.
