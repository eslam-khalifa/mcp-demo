# MCP Demo: Platzi Fake Store API Integration

This project is a demonstration of an MCP (Model Context Protocol) server built using .NET 8. It provides local tools for LLMs to interact with the Platzi Fake Store API.

## Project Structure

- `src/`: Solution source code.
  - `MCPDemo.Domain`: Core entities and exceptions.
  - `MCPDemo.Application`: Service interfaces, DTOs, and business logic.
  - `MCPDemo.Infrastructure`: External API clients, logging, and metrics implementations.
  - `MCPDemo.Shared`: Cross-cutting concerns like the `Result<T>` pattern.
- `tests/`: Unit and integration tests.
- `specs/`: Phase-by-phase implementation plans and documentation.

## Current Progress

### Phase 1: Solution Setup ✅
- [x] Multi-project architecture (.NET 8).
- [x] Shared patterns and base entities.

### Phase 2: Core Services & Infrastructure ✅
- [x] Robust API Client with retries.
- [x] Product, Category, and Search services.
- [x] Structured logging and runtime metrics.

## Next Steps
- **Phase 3**: Tool Definitions & Handlers (MCP protocol integration).
- **Phase 4**: Security & Rate Limiting.
