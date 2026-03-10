# Feature Specification: MCP Server & C# Tool Definitions

**Feature**: `3-mcp-server-tools`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: Phase 3 from implementation-plan.md — Bootstrap MCP server (stdio) and implement Product, Category, and Search MCP tool classes.

## Clarifications

### Session 2026-03-10

- Q: Should error responses from MCP tools be structured JSON or plain text strings? → A: Success returns serialized JSON entity; Failure returns a plain error message string (not JSON-wrapped). JSON serialization policy applies only to success payloads.

## Assumptions

- Phase 1 (Foundation & Setup) and Phase 2 (Core Services & Infrastructure) are complete.
- The `ModelContextProtocol` NuGet SDK is already installed in `MCPDemo.Api`.
- All Application-layer service interfaces (`IProductService`, `ICategoryService`, `ISearchService`) and their implementations exist and are operational.
- The `Result<T>` pattern is used by all services; MCP tools must unwrap results before returning JSON.
- The MCP server uses **stdio** transport (not HTTP) for communication with Cursor IDE.
- Tools are discovered automatically via `WithToolsFromAssembly()`.
- Each tool class is a static class decorated with `[McpServerToolType]`.
- Each tool method is a static async method decorated with `[McpServerTool]` and `[Description]`.
- Tool methods receive services via method-level DI (parameters resolved by the MCP SDK).
- Tool methods return `string` (serialized JSON).

## User Scenarios & Testing *(mandatory)*

### User Story 1 — MCP Server Bootstrap (Priority: P1)

The MCP server must start and listen for JSON-RPC tool calls over stdio. When the AI assistant in Cursor IDE connects, it should discover all registered tools automatically and be ready to invoke them.

**Why this priority**: Without a running MCP server, no tools can be invoked. This is the foundation for all other stories.

**Independent Test**: Start the MCP server process. Verify it initializes without errors and that stdio transport is active. Tool discovery can be validated by checking the server's tool listing response.

**Acceptance Scenarios**:

1. **Given** the MCP server is started, **When** the host initializes, **Then** all registered services (Product, Category, Search, Metrics) are available via DI and the server listens on stdio without errors.
2. **Given** the MCP server is running, **When** a tool listing request is sent, **Then** all registered MCP tools (product, category, search) are returned with their names and descriptions.
3. **Given** the MCP server is running, **When** a tool call request is sent for a registered tool, **Then** the request is routed to the correct tool method via the MCP SDK.

---

### User Story 2 — Product MCP Tools (Priority: P1)

An AI assistant needs to perform all product operations (list, get by ID, get by slug, create, update, delete, get related) through MCP tools. Each tool is a thin wrapper that delegates to `IProductService`, unwraps the `Result<T>`, and returns serialized JSON.

**Why this priority**: Products are the primary data entity; product tools enable the core use case of the MCP server.

**Independent Test**: Invoke each product tool via a mock MCP call. Verify that valid inputs produce correct JSON output and that error results produce descriptive error messages.

**Acceptance Scenarios**:

1. **Given** the MCP server is running, **When** `get_all_products` is called with optional `offset` and `limit`, **Then** a JSON array of products is returned.
2. **Given** the MCP server is running, **When** `get_product_by_id` is called with a valid `id`, **Then** a single product JSON object is returned.
3. **Given** the MCP server is running, **When** `get_product_by_id` is called with a non-existent `id`, **Then** an error message indicating "not found" is returned.
4. **Given** the MCP server is running, **When** `get_product_by_slug` is called with a valid `slug`, **Then** a single product JSON object is returned.
5. **Given** the MCP server is running, **When** `create_product` is called with valid inputs (title, price, description, categoryId, images), **Then** the created product JSON is returned.
6. **Given** the MCP server is running, **When** `create_product` is called with invalid inputs (e.g., empty title), **Then** a validation error message is returned.
7. **Given** the MCP server is running, **When** `update_product` is called with a valid `id` and at least one field to update, **Then** the updated product JSON is returned.
8. **Given** the MCP server is running, **When** `delete_product` is called with a valid `id`, **Then** a success confirmation is returned.
9. **Given** the MCP server is running, **When** `get_related_products_by_id` is called with a valid `id`, **Then** a JSON array of related products is returned.
10. **Given** the MCP server is running, **When** `get_related_products_by_slug` is called with a valid `slug`, **Then** a JSON array of related products is returned.

---

### User Story 3 — Category MCP Tools (Priority: P2)

An AI assistant needs to perform all category operations (list, get by ID, get by slug, create, update, delete, get products in category) through MCP tools. Each tool delegates to `ICategoryService`.

**Why this priority**: Categories are supporting entities; they are important but secondary to products.

**Independent Test**: Invoke each category tool. Verify correct JSON output for valid inputs and descriptive error messages for invalid inputs.

**Acceptance Scenarios**:

1. **Given** the MCP server is running, **When** `get_all_categories` is called, **Then** a JSON array of all categories is returned.
2. **Given** the MCP server is running, **When** `get_category_by_id` is called with a valid `id`, **Then** a single category JSON object is returned.
3. **Given** the MCP server is running, **When** `get_category_by_slug` is called with a valid `slug`, **Then** a single category JSON object is returned.
4. **Given** the MCP server is running, **When** `create_category` is called with valid inputs (name, image), **Then** the created category JSON is returned.
5. **Given** the MCP server is running, **When** `update_category` is called with a valid `id` and at least one field, **Then** the updated category JSON is returned.
6. **Given** the MCP server is running, **When** `delete_category` is called with a valid `id`, **Then** a success confirmation is returned.
7. **Given** the MCP server is running, **When** `get_products_by_category` is called with a valid `categoryId`, **Then** a JSON array of products in that category is returned.

---

### User Story 4 — Search MCP Tool (Priority: P2)

An AI assistant needs to search and filter products using combinable criteria (title, price range, category, pagination) through a single `search_products` MCP tool.

**Why this priority**: Search is a key capability for the AI to explore the store, but it builds on products and categories.

**Independent Test**: Call `search_products` with various filter combinations. Verify that filters are correctly applied and results are returned as JSON.

**Acceptance Scenarios**:

1. **Given** the MCP server is running, **When** `search_products` is called with no filters, **Then** all products are returned (equivalent to get all).
2. **Given** the MCP server is running, **When** `search_products` is called with `title` filter, **Then** only matching products are returned.
3. **Given** the MCP server is running, **When** `search_products` is called with `price_min` and `price_max`, **Then** only products within the price range are returned.
4. **Given** the MCP server is running, **When** `search_products` is called with `categoryId` and `limit`, **Then** the response is correctly filtered and paginated.

---

### Edge Cases

- What happens when a tool receives a `Result.Failure` from the service layer? → The tool must return the error message as a string (not throw).
- What happens if JSON serialization of a result fails? → The tool should catch the exception and return a descriptive error string.
- What happens if an MCP tool method receives a null or missing required parameter? → The MCP SDK handles parameter validation based on method signatures; non-nullable parameters are required.
- What happens if the MCP server fails to start (e.g., port conflict, missing DI registration)? → The process should log the error and exit with a non-zero code.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST bootstrap an MCP server using stdio transport in `Program.cs`.
- **FR-002**: System MUST register all Application services (`IProductService`, `ICategoryService`, `ISearchService`) and Infrastructure services (`IPlatziStoreApiClient`, `IMetricsCollector`) in the DI container.
- **FR-003**: System MUST configure `HttpClient` for `IPlatziStoreApiClient` with base URL `https://api.escuelajs.co/api/v1/` and a 15-second timeout.
- **FR-004**: System MUST discover MCP tools automatically via `WithToolsFromAssembly()`.
- **FR-005**: System MUST expose 8 product MCP tools: `get_all_products`, `get_product_by_id`, `get_product_by_slug`, `create_product`, `update_product`, `delete_product`, `get_related_products_by_id`, `get_related_products_by_slug`.
- **FR-006**: System MUST expose 7 category MCP tools: `get_all_categories`, `get_category_by_id`, `get_category_by_slug`, `create_category`, `update_category`, `delete_category`, `get_products_by_category`.
- **FR-007**: System MUST expose 1 search MCP tool: `search_products` with all optional filter parameters.
- **FR-008**: Each MCP tool MUST delegate to the corresponding Application-layer service method.
- **FR-009**: Each MCP tool MUST unwrap the `Result<T>` response: if success, serialize the value to JSON using the configured serializer; if failure, return the error message as a plain text string (not JSON-wrapped).
- **FR-010**: Each MCP tool MUST have a `[Description]` attribute with a clear, natural-language description for AI discoverability.
- **FR-011**: Each MCP tool parameter MUST have a `[Description]` attribute explaining its purpose and constraints.
- **FR-012**: System MUST configure Serilog for structured logging during server bootstrap.
- **FR-013**: System MUST register `IMetricsCollector` as a singleton for runtime metrics.
- **FR-014**: Tool methods MUST use `System.Text.Json` with camelCase naming policy for serializing **success** payloads. Error/failure responses are plain text strings.
- **FR-015**: Tool methods MUST NOT throw exceptions; all errors must be returned as string messages.
- **FR-016**: The `create_product` tool MUST accept parameters: `title` (string), `price` (decimal), `description` (string), `categoryId` (int), `images` (string array).
- **FR-017**: The `update_product` tool MUST accept parameters: `id` (int), plus optional `title`, `price`, `description`, `categoryId`, `images`.
- **FR-018**: The `search_products` tool MUST accept all optional parameters: `title`, `price`, `priceMin`, `priceMax`, `categoryId`, `categorySlug`, `offset`, `limit`.

### Key Entities

- **MCP Tool**: A discoverable operation exposed via the MCP protocol. Identified by name, description, input parameters, and output format.
- **MCP Server**: The stdio-based process that receives JSON-RPC requests, routes them to tool methods, and returns responses.
- **DI Container**: The service collection that provides Application and Infrastructure services to tool methods at invocation time.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The MCP server starts successfully and is ready to accept tool calls within 5 seconds.
- **SC-002**: All 16 MCP tools (8 product + 7 category + 1 search) are discoverable by the AI assistant.
- **SC-003**: Each tool call completes and returns a valid JSON response or descriptive error message within the 15-second timeout.
- **SC-004**: Tool calls with invalid inputs return clear, human-readable error messages (not stack traces).
- **SC-005**: The solution builds with zero errors and zero warnings after Phase 3 implementation.
- **SC-006**: All tool descriptions are clear enough for an AI assistant to understand when and how to use each tool without additional documentation.
