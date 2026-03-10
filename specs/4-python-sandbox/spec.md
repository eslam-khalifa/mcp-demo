# Feature Specification: Python Sandbox & Custom Code Execution

**Feature Branch**: `4-python-sandbox`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: User description: "Phase 4 from the implementation plan — general-purpose Python code execution in a secure sandbox for custom analytics on store data"

## User Scenarios & Testing

### User Story 1 - Run Custom Analytics on Store Data (Priority: P1)

As an AI assistant user, I want to run custom Python code against product and category data so that I can answer any analytical question about the store — not just the ones covered by pre-built tools.

**Why this priority**: This is the core value of the feature. Without the ability to execute arbitrary code, the system is limited to the 16 fixed CRUD/search tools and cannot answer complex analytical questions like "What is the average price of the top 5 most expensive laptops?" or "What percentage of products cost less than $50?"

**Independent Test**: Can be fully tested by fetching product data with an existing tool (e.g., `get_all_products`), then passing that data along with a Python script to `run_python_code`, and verifying that the script's output is returned correctly.

**Acceptance Scenarios**:

1. **Given** the AI assistant has fetched product data using `get_all_products`, **When** it submits Python code that sorts products by price descending, takes the top 5, and computes their average price, **Then** the tool returns a JSON string containing the top 5 products and the computed average.
2. **Given** the AI assistant has fetched product data, **When** it submits Python code that counts the number of products priced below $50, **Then** the tool returns the correct count.
3. **Given** the AI assistant has fetched category data, **When** it submits Python code that computes summary statistics per category, **Then** the tool returns a JSON string with per-category min, max, average, and count.

---

### User Story 2 - Secure Execution Environment (Priority: P1)

As a system operator, I want the Python code execution to be sandboxed so that untrusted code cannot access the network, read/write the host filesystem, or consume excessive resources.

**Why this priority**: Security is a hard requirement. The sandbox executes AI-generated code, which must be isolated from the host system regardless of what the code attempts.

**Independent Test**: Can be tested by submitting code that attempts to access the network (e.g., `import urllib`), read files (e.g., `open('/etc/passwd')`), or run indefinitely (e.g., `while True: pass`), and verifying that all these attempts fail gracefully.

**Acceptance Scenarios**:

1. **Given** the sandbox is running, **When** submitted code attempts to make a network request, **Then** the execution fails and returns an error message.
2. **Given** the sandbox is running, **When** submitted code attempts to read or write files on the host filesystem, **Then** the execution fails and returns an error message.
3. **Given** the sandbox is running, **When** submitted code runs for longer than the timeout limit (30 seconds), **Then** the execution is terminated and a timeout error is returned.
4. **Given** the sandbox is running, **When** submitted code exceeds the memory limit (256 MB), **Then** the execution is terminated and a resource error is returned.

---

### User Story 3 - Error Handling and Feedback (Priority: P2)

As an AI assistant, I want clear error messages when my Python code fails so that I can understand what went wrong and correct my code.

**Why this priority**: The AI assistant generates code at runtime, and errors are expected. Clear, actionable feedback allows the AI to self-correct and retry.

**Independent Test**: Can be tested by submitting code with various error types (syntax errors, runtime exceptions, missing variables) and verifying that the returned error messages are descriptive and actionable.

**Acceptance Scenarios**:

1. **Given** the AI submits Python code with a syntax error, **When** the sandbox attempts to execute it, **Then** the tool returns an error message that includes the error type and a description of the problem.
2. **Given** the AI submits Python code that raises a runtime exception (e.g., division by zero), **When** the sandbox executes it, **Then** the tool returns an error message with the exception type and message.
3. **Given** the AI submits an empty code string, **When** the tool is invoked, **Then** the tool returns a validation error before attempting execution.

---

### Edge Cases

- What happens when the `data` parameter is empty or `null`? The code should still execute, with the `data` variable set to `None`.
- What happens when the Python code produces no output (no `print()` calls)? The tool should return an empty string.
- What happens when the Python code outputs very large results? The output should be captured up to a reasonable limit and truncated if necessary.
- What happens when Docker is not installed or the sandbox image is not built? The tool should return a clear error indicating the sandbox is unavailable.
- What happens when the `data` parameter is not valid JSON? The tool should return a descriptive parse error before attempting execution.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST provide a single MCP tool named `run_python_code` that accepts two parameters: `code` (the Python source to execute) and `data` (a JSON string containing the data to process).
- **FR-002**: The system MUST execute the submitted Python code in an isolated sandbox environment with no network access.
- **FR-003**: The system MUST make the `data` parameter available to the Python code as a pre-parsed variable (not a raw string) so the code can use it directly.
- **FR-004**: The system MUST provide the `pandas` and `numpy` libraries within the sandbox for data analysis.
- **FR-005**: The system MUST capture all standard output (`print()` calls) from the Python code and return it as the tool's result.
- **FR-006**: The system MUST enforce a memory limit of 256 MB on sandbox execution.
- **FR-007**: The system MUST enforce a CPU limit of 0.5 cores on sandbox execution.
- **FR-008**: The system MUST enforce a maximum execution time of 30 seconds; exceeding this terminates the process and returns a timeout error.
- **FR-009**: The system MUST prevent the sandbox from writing to the host filesystem (read-only filesystem).
- **FR-010**: The system MUST prevent privilege escalation within the sandbox.
- **FR-011**: The system MUST restrict the Python builtins available in the sandbox to a safe subset — no `open`, `exec` (nested), `eval`, `__import__`, `compile`, `globals`, `locals`, or file/OS operations.
- **FR-012**: The system MUST validate that the `code` parameter is not empty before attempting execution.
- **FR-013**: The system MUST return descriptive error messages when execution fails, including the error type and message from the Python runtime.
- **FR-014**: The system MUST log each `run_python_code` invocation (execution time, success/failure, error type if applicable) using the existing metrics and logging infrastructure.

### Key Entities

- **Sandbox Execution Request**: The combination of Python source code and a JSON data payload submitted for execution.
- **Sandbox Execution Result**: The captured standard output from the Python code, or an error message if execution failed.
- **Sandbox Environment**: The isolated runtime (container) where Python code executes with restricted access — no network, read-only filesystem, limited CPU/memory, safe builtins only.

## Success Criteria

### Measurable Outcomes

- **SC-001**: The AI assistant can answer any ad-hoc analytical question about store data by generating and executing custom Python code within 30 seconds.
- **SC-002**: 100% of network access attempts from within the sandbox are blocked and result in an error.
- **SC-003**: 100% of filesystem write attempts from within the sandbox are blocked and result in an error.
- **SC-004**: Python code execution results are returned to the AI assistant within 30 seconds for typical analytics workloads (sorting, filtering, aggregation on up to 1,000 products).
- **SC-005**: When Python code fails (syntax or runtime errors), the returned error message is sufficient for the AI to understand the cause and self-correct on the next attempt.
- **SC-006**: The sandbox environment consumes no more than 256 MB of memory and 0.5 CPU cores regardless of the submitted code.
- **SC-007**: The system correctly handles edge cases (empty code, null data, invalid JSON, no output, Docker unavailable) with descriptive error messages rather than crashes.

## Assumptions

- Docker is installed and available on the host machine where the MCP server runs.
- The Docker image for the Python sandbox is pre-built before the MCP server starts (build is a one-time setup step, not a runtime operation).
- The AI assistant is responsible for fetching data using existing MCP tools before calling `run_python_code` — the sandbox tool does not fetch data itself.
- The `pandas` and `numpy` library versions are fixed at build time and do not need to be configurable at runtime.
- The sandbox executes one script at a time (no concurrent execution within a single sandbox instance, though multiple sandbox containers may run concurrently via separate tool calls).

## Dependencies

- **Phase 3 (MCP Server & C# Tools)**: Must be complete — the `run_python_code` tool is registered within the same MCP server and relies on the existing DI, logging, and metrics infrastructure.
- **Docker**: Required on the host for sandbox execution.
- **Existing MCP tools**: The AI uses `get_all_products`, `search_products`, `get_all_categories`, etc. to fetch data before passing it to `run_python_code`.

## Out of Scope

- Pre-built analytics scripts or tool shortcuts (all analytics are AI-generated at runtime).
- Persistent state between `run_python_code` calls (each execution is stateless).
- Custom library installation at runtime (only `pandas`, `numpy`, and Python stdlib are available).
- Graphical output (charts, plots) — output is text/JSON only.
- Web-based code editor or REPL for the sandbox.
