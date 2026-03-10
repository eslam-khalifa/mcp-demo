# Tasks: MCP Server & C# Tool Definitions

**Input**: Design documents from `specs/3-mcp-server-tools/`
**Prerequisites**: plan.md (✅), spec.md (✅), research.md (✅), data-model.md (✅), contracts/ (✅)

**Tests**: Not explicitly requested — test tasks omitted.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (NuGet Packages & Project Configuration)

**Purpose**: Install required NuGet packages and prepare the Api project for MCP server integration.

- [x] T001 Add `ModelContextProtocol` NuGet package to `src/MCPDemo.Api/MCPDemo.Api.csproj`
- [x] T002 [P] Add `Microsoft.Extensions.Hosting` NuGet package to `src/MCPDemo.Api/MCPDemo.Api.csproj`
- [x] T003 [P] Add `Serilog.AspNetCore` NuGet package to `src/MCPDemo.Api/MCPDemo.Api.csproj`
- [x] T004 Create directory `src/MCPDemo.Api/McpTools/` for MCP tool definitions
- [x] T005 Verify solution builds: `dotnet build MCPDemo.sln` — zero errors

**Checkpoint**: Api project has all required NuGet packages and the McpTools directory exists.

---

## Phase 2: Foundational (MCP Server Bootstrap)

**Purpose**: Bootstrap the MCP server with stdio transport and DI wiring. This MUST complete before any tool can be implemented.

**⚠️ CRITICAL**: No tool implementation can begin until this phase is complete.

- [x] T006 Implement `Program.cs` in `src/MCPDemo.Api/Program.cs` — replace placeholder with `Host.CreateApplicationBuilder`, register all Application services (`IProductService` → `ProductService`, `ICategoryService` → `CategoryService`, `ISearchService` → `SearchService`), register `IMetricsCollector` → `InMemoryMetricsCollector` as singleton
- [x] T007 Configure `HttpClient` registration in `src/MCPDemo.Api/Program.cs` — register `IPlatziStoreApiClient` → `PlatziStoreApiClient` with base URL `https://api.escuelajs.co/api/v1/` and 15-second timeout via `AddHttpClient`
- [x] T008 Configure Serilog integration in `src/MCPDemo.Api/Program.cs` — integrate with `SerilogConfiguration` from Infrastructure layer
- [x] T009 Register MCP server in `src/MCPDemo.Api/Program.cs` — call `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`
- [x] T010 Define shared `JsonSerializerOptions` in `src/MCPDemo.Api/McpTools/ToolJsonOptions.cs` — static field with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, reused by all tool classes
- [x] T011 Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: MCP server starts, DI container resolves all services, stdio transport is active. No tools registered yet.

---

## Phase 3: User Story 1 — MCP Server Bootstrap & Product Tools (Priority: P1) 🎯 MVP

**Goal**: Expose all 8 product operations as MCP tools. The AI assistant can discover and invoke product CRUD tools.

**Independent Test**: Start the MCP server. Invoke `get_product_by_id` with id=1 via stdio. Verify a valid JSON product is returned.

### Implementation for User Story 1

- [x] T012 [US1] Implement `get_all_products` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — static method with `[McpServerTool]` and `[Description("List all products with optional pagination")]`, parameters: `int? offset`, `int? limit`; delegates to `IProductService.GetAllAsync`; unwrap `Result<T>`: success → JSON, failure → plain string
- [x] T013 [US1] Implement `get_product_by_id` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameter: `int id` with `[Description("The product ID")]`; delegates to `IProductService.GetByIdAsync`
- [x] T014 [US1] Implement `get_product_by_slug` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameter: `string slug`; delegates to `IProductService.GetBySlugAsync`
- [x] T015 [US1] Implement `create_product` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameters: `string title`, `decimal price`, `string description`, `int categoryId`, `string[] images`; constructs `CreateProductDto` and delegates to `IProductService.CreateAsync`
- [x] T016 [US1] Implement `update_product` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameters: `int id`, `string? title`, `decimal? price`, `string? description`, `int? categoryId`, `string[]? images`; constructs `UpdateProductDto` and delegates to `IProductService.UpdateAsync`
- [x] T017 [US1] Implement `delete_product` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameter: `int id`; delegates to `IProductService.DeleteAsync`
- [x] T018 [US1] Implement `get_related_products_by_id` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameter: `int id`; delegates to `IProductService.GetRelatedByIdAsync`
- [x] T019 [US1] Implement `get_related_products_by_slug` tool in `src/MCPDemo.Api/McpTools/ProductTools.cs` — parameter: `string slug`; delegates to `IProductService.GetRelatedBySlugAsync`
- [x] T020 [US1] Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: 8 product MCP tools are registered and discoverable. AI assistant can perform all product operations.

---

## Phase 4: User Story 2 — Category MCP Tools (Priority: P2)

**Goal**: Expose all 7 category operations as MCP tools. The AI assistant can discover and invoke category CRUD tools.

**Independent Test**: Invoke `get_all_categories` via stdio. Verify a JSON array of categories is returned.

### Implementation for User Story 2

- [x] T021 [P] [US2] Implement `get_all_categories` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — static method with `[McpServerTool]` and `[Description("List all product categories")]`; delegates to `ICategoryService.GetAllAsync`; unwrap `Result<T>`: success → JSON, failure → plain string
- [x] T022 [P] [US2] Implement `get_category_by_id` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameter: `int id`; delegates to `ICategoryService.GetByIdAsync`
- [x] T023 [P] [US2] Implement `get_category_by_slug` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameter: `string slug`; delegates to `ICategoryService.GetBySlugAsync`
- [x] T024 [US2] Implement `create_category` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameters: `string name`, `string image`; constructs `CreateCategoryDto` and delegates to `ICategoryService.CreateAsync`
- [x] T025 [US2] Implement `update_category` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameters: `int id`, `string? name`, `string? image`; constructs `UpdateCategoryDto` and delegates to `ICategoryService.UpdateAsync`
- [x] T026 [US2] Implement `delete_category` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameter: `int id`; delegates to `ICategoryService.DeleteAsync`
- [x] T027 [US2] Implement `get_products_by_category` tool in `src/MCPDemo.Api/McpTools/CategoryTools.cs` — parameter: `int categoryId`; delegates to `ICategoryService.GetProductsAsync`
- [x] T028 [US2] Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: 7 category MCP tools are registered and discoverable. AI assistant can perform all category operations.

---

## Phase 5: User Story 3 — Search MCP Tool (Priority: P2)

**Goal**: Expose the search tool for filtering products with combinable criteria.

**Independent Test**: Invoke `search_products` with `title=Generic` via stdio. Verify filtered results are returned.

### Implementation for User Story 3

- [x] T029 [US3] Implement `search_products` tool in `src/MCPDemo.Api/McpTools/SearchTools.cs` — static method with `[McpServerTool]` and `[Description("Search and filter products using combinable criteria. All parameters are optional.")]`; parameters: `string? title`, `decimal? price`, `decimal? priceMin`, `decimal? priceMax`, `int? categoryId`, `string? categorySlug`, `int? offset`, `int? limit`; constructs `SearchProductsDto` and delegates to `ISearchService.SearchProductsAsync`; unwrap `Result<T>`: success → JSON, failure → plain string
- [x] T030 [US3] Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: All 16 MCP tools (8 product + 7 category + 1 search) are registered and discoverable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, cleanup, and documentation.

- [x] T031 Verify full solution build with warnings-as-errors: `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` — zero errors, zero warnings
- [x] T032 [P] Verify all 16 tool methods have `[Description]` attributes on both the method and all parameters in `src/MCPDemo.Api/McpTools/ProductTools.cs`, `CategoryTools.cs`, `SearchTools.cs`
- [x] T033 [P] Add XML documentation comments to all public methods in `src/MCPDemo.Api/McpTools/ProductTools.cs`, `CategoryTools.cs`, `SearchTools.cs`, and `ToolJsonOptions.cs`
- [x] T034 [P] Verify `Program.cs` registers all required DI services — cross-check with `data-model.md` DI registration table
- [x] T035 Run `quickstart.md` validation: all key files exist, solution builds, no architecture violations

**Checkpoint**: Phase 3 feature complete. All 16 MCP tools operational. Solution builds with zero warnings.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1: Product Tools (Phase 3)**: Depends on Foundational (needs DI + MCP server bootstrap)
- **US2: Category Tools (Phase 4)**: Depends on Foundational (needs DI + MCP server bootstrap)
- **US3: Search Tool (Phase 5)**: Depends on Foundational (needs DI + MCP server bootstrap)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 → Phase 2 → US1 (P1) ─────────────┐
                 │                          │
                 ├──→ US2 (P2, parallel) ───┼──→ Polish
                 │                          │
                 └──→ US3 (P2, parallel) ───┘
```

- **US1 (Product Tools)**: MVP — implement first
- **US2 (Category Tools)**: Can run in PARALLEL with US1 after Phase 2
- **US3 (Search Tool)**: Can run in PARALLEL with US1/US2 after Phase 2
- **US2 and US3** are independent of US1

### Parallel Opportunities

**Within Phase 1**: T001–T004 can all run in parallel (different NuGet packages / directory)

**After Phase 2 completes**: US1, US2, US3 can all start in parallel:
```
US1 tasks (T012–T020) in parallel with
US2 tasks (T021–T028) in parallel with
US3 tasks (T029–T030)
```

**Within US2**: T021, T022, T023 are parallelizable (read-only tools in same file, but independent methods)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (NuGet packages)
2. Complete Phase 2: Foundational (Program.cs bootstrap)
3. Complete Phase 3: US1 — Product Tools
4. **STOP and VALIDATE**: Start the MCP server, invoke `get_product_by_id` with id=1
5. Product tools are the MVP — they enable the primary AI assistant use case

### Incremental Delivery

1. Setup + Foundational → MCP server starts (no tools yet)
2. US1 (Product Tools) → AI can manage products (MVP!)
3. US2 (Category Tools) → AI can manage categories
4. US3 (Search Tool) → AI can search and filter products
5. Polish → Production readiness

---

## Notes

- All tools are in the `src/MCPDemo.Api/McpTools/` directory
- All tools are static classes with `[McpServerToolType]` attribute
- All tool methods follow the same pattern: unwrap `Result<T>`, serialize success to JSON, return failure as plain string
- `ToolJsonOptions.cs` provides shared JSON serialization options (CamelCase)
- Tool methods accept services via method-level DI parameters (resolved by MCP SDK at invocation)
- Total tasks: 35
