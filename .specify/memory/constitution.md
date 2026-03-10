<!--
  ============================================================
  SYNC IMPACT REPORT
  ============================================================
  Version change: N/A (initial) → 1.0.0
  Modified principles: N/A (initial creation)
  Added sections:
    - Core Principles (5): MCP Tool Standards, API Calling & Efficiency,
      Logging & Metrics, Clean Architecture, Security & Sandboxing
    - Technology Stack & Python Integration
    - Spec-Kit Conventions & Versioning
    - Governance
  Removed sections: N/A
  Templates requiring updates:
    - .specify/templates/plan-template.md        ✅ compatible (Constitution Check section exists)
    - .specify/templates/spec-template.md         ✅ compatible (no constitution-specific refs)
    - .specify/templates/tasks-template.md        ✅ compatible (no constitution-specific refs)
  Follow-up TODOs: None
  ============================================================
-->

# MCP Platzi Fake Store API Demo Constitution

## Core Principles

### I. MCP Tool Standards

Every MCP tool MUST adhere to a strict definition contract and category classification.

- **Definition contract**: Each tool MUST declare a unique **Name**, a human-readable
  **Description**, typed **Inputs**, typed **Outputs**, a **Category**
  (CRUD | Search | Filter | Reporting | Python Analysis), and an
  **Implementation Type** (C# | Python).
- **CRUD Operations**: Create, read, update, and delete products, categories, or
  other Platzi Fake Store resources. All input data MUST be validated before any
  external API call is made.
- **Searching & Filtering**: Search by name, category, price range, rating, or
  other attributes. Filters MUST be combinable and paginated where the data set
  may exceed a single page.
- **Reporting & Analytics**: Generate aggregated data (totals, averages, min/max).
  Python tools MAY be used for advanced analytical computations not directly
  available through API endpoints.
- **Hybrid Python Tools**: Execute analysis inside sandboxed Docker containers
  (`python:3.11-slim`). Resource limits: 256 MB memory, 0.5 CPU cores, 30-second
  timeout. Inputs and outputs MUST be serialized to JSON. No network access
  outside the MCP backend API unless explicitly allowed.
- Tools MUST use the backend API endpoints whenever a suitable endpoint exists;
  Python tools are reserved for computations the API cannot provide directly.

### II. API Calling & Efficiency

External API calls to the Platzi Fake Store API MUST be minimized and resilient.

- **Caching**: A dummy/local cache layer MUST store API responses for reuse to
  reduce redundant network calls.
- **Retry mechanism**: On transient failures the system MUST retry up to
  **2 additional attempts** before propagating the error.
- **Error handling**: Every API error MUST be caught, classified, and propagated
  to the MCP tool output with a structured error payload (error code, message,
  timestamp).
- No raw exception details from external APIs are permitted in tool output;
  errors MUST be wrapped in a domain-specific error model.

### III. Logging & Metrics

All MCP tool executions MUST be observable through file-based logging and
runtime metrics.

- **File logging**: Every tool execution MUST produce a log entry containing
  timestamp (ISO 8601), tool name, input parameters, output summary, and
  wall-clock execution time in milliseconds.
- **Metrics** tracked at runtime:
  - Total count of MCP tool calls (per tool and aggregate).
  - Average execution time per tool.
  - Count of Python sandbox executions.
  - Count and classification of failed executions by error type.
- Log files MUST NOT contain sensitive information (API keys, user credentials,
  PII).

### IV. Clean Architecture

The codebase MUST follow a layered architecture with strict dependency direction.

1. **API Layer** — Exposes HTTP endpoints consumed by MCP tools. No business
   logic resides here.
2. **Application Layer** — Orchestrates MCP tool logic. All MCP tool
   implementations MUST reside in this layer.
3. **Domain Layer** — Contains business logic, entities, and value objects.
   No infrastructure dependencies allowed.
4. **Infrastructure Layer** — Handles external API calls (Platzi Fake Store),
   file logging, metrics collection, caching, and Python sandbox integration.
- Dependencies MUST flow inward: API → Application → Domain ← Infrastructure.
- Python integration is classified as an **infrastructure service** and MUST
  be accessed only through application-layer abstractions.

### V. Security & Sandboxing (NON-NEGOTIABLE)

All execution boundaries MUST be hardened against untrusted input and resource
abuse.

- Python tools MUST NEVER execute code outside a Docker sandbox container.
- All input parameters MUST be validated (type, range, length) before execution
  in both C# and Python code paths.
- System resource access (filesystem, network, environment variables) MUST be
  restricted to the minimum required by each tool.
- Log output MUST be sanitized; no secrets, tokens, or credentials are permitted
  in log files.
- Docker containers MUST run with **no-new-privileges** and a read-only root
  filesystem where feasible.

## Technology Stack & Python Integration

- **Backend**: .NET 8, C#, ASP.NET Core Web API, Entity Framework Core, LINQ.
- **External Data Source**: [Platzi Fake Store API](https://fakeapi.platzi.com/)
  — the primary and only external data source for all MCP tools.
- **Python Runtime**: Python 3.11 inside `python:3.11-slim` Docker images.
  Used exclusively for advanced analysis/computation not provided by API
  endpoints.
- **MCP Server**: Connected to Cursor IDE for MCP execution and tool discovery.
- **Logging**: File-based structured logging (JSON format recommended).
- **Metrics**: In-process counters exposed through the Application Layer.

Python sandbox constraints (enforced by Docker):

| Resource | Limit |
|----------|-------|
| Memory   | 256 MB |
| CPU      | 0.5 cores |
| Timeout  | 30 seconds |
| Network  | MCP backend API only (no public internet) |

## Spec-Kit Conventions & Versioning

### Spec-Kit File Rules

- Every MCP tool (C# or Python) MUST have an individual `.spec.md` file
  following the spec-kit format: **name, description, inputs, outputs,
  category, and implementation type**.
- This constitution file serves as the **foundation for all spec-kit
  validations**; every spec MUST be consistent with the principles defined here.

### Versioning Policy

- Any new MCP tool or modification to an existing tool MUST increment the
  **version** field in its spec file.
- Version numbers follow **semantic versioning** (MAJOR.MINOR.PATCH):
  - MAJOR: Backward-incompatible changes to tool inputs/outputs.
  - MINOR: New capabilities or optional parameters added.
  - PATCH: Bug fixes, documentation, or internal refactoring with no
    contract change.

### General Guidelines

- Focus on the **Platzi Fake Store API** as the sole external data source.
- Python tools are optional but recommended for advanced analytical scenarios.
- All code MUST be **modular, testable, and production-ready**.

## Governance

- This constitution **supersedes** all other project-level practices and
  conventions. In case of conflict, this document is authoritative.
- **Phased implementation review**: During implementation (`/speckit.implement`),
  the AI MUST NOT proceed to the next phase until the user has explicitly
  reviewed and approved the current phase. After completing each phase, the AI
  MUST stop, report what was done, and wait for the user's confirmation before
  continuing.
- **Amendment procedure**:
  1. Propose changes in a pull request with a clear rationale.
  2. Changes MUST be reviewed and approved by the project author before
     adoption.
  3. Every amendment MUST update the version footer below and include a
     migration plan if principles are removed or redefined.
- **Compliance review**: All pull requests and code reviews MUST verify
  adherence to the principles in this constitution. Non-compliance MUST be
  flagged and resolved before merge.
- Use the spec-kit workflow commands (`/speckit.*`) for runtime development
  guidance and to ensure all artifacts remain consistent with this
  constitution.

**Version**: 1.1.0 | **Ratified**: 2026-03-09 | **Last Amended**: 2026-03-10
