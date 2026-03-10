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

### Phase 3: Infrastructure & Sandbox ✅
- [x] Docker-based Python sandbox with strict security.
- [x] In-memory thread-safe metrics collector.
- [x] Integration with Platzi Store API.

### Phase 4: Testing & Quality Assurance ✅
- [x] Comprehensive Unit Test suite (Application & Infrastructure).
- [x] Integration Tests for live API and Docker sandbox.
- [x] Fixed Critical Bugs: Pagination error in API, improved error capture in Sandbox.

## Next Steps
- **Phase 5**: Tool Definitions & Handlers (MCP protocol integration).
- **Phase 6**: Security & Rate Limiting.

