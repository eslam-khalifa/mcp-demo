# MCP Platzi Fake Store API Demo

A high-performance **Model Context Protocol (MCP)** server built with .NET 8. This server provides a suite of 18 tools that allow AI assistants (like Cursor, Claude, or ChatGPT) to interact with the Platzi Fake Store API for e-commerce operations, data analysis, and system monitoring.

## 🚀 Key Features

- **18 MCP Tools**: Complete CRUD for Products and Categories, advanced Search, and Runtime Metrics.
- **Secure Python Sandbox**: A Docker-based isolated environment for complex data analysis using `pandas` and `numpy`.
- **Clean Architecture**: Decoupled design with Domain, Application, Infrastructure, and API layers.
- **Production-Ready**: Implementation of retries, structured error handling, and session-based metrics.

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed:
1. **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Docker Desktop**: Required for the `run_python_code` tool. [Download here](https://www.docker.com/products/docker-desktop/)
3. **MCP Client**: An IDE or host that supports MCP (e.g., [Cursor](https://cursor.com), [Claude Desktop](https://claude.ai/download)).

---

## ⚡ Quick Start

Follow these 5 steps to get the MCP server running in your environment:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/eslam-khalifa/mcp-demo.git
   cd mcp-demo
   ```

2. **Restore and Build**:
   ```bash
   dotnet restore MCPDemo.sln
   dotnet build MCPDemo.sln -c Release
   ```

3. **Pull Python Sandbox Image**:
   ```bash
   docker build -t mcp-python-sandbox:latest -f src/MCPDemo.Api/Dockerfile.sandbox .
   ```

4. **Configure MCP Client**:
   Add the following to your MCP settings (e.g., `cursor.json` or `claude_desktop_config.json`):
   ```json
   {
     "mcpServers": {
       "platzi-store": {
         "command": "dotnet",
         "args": ["run", "--project", "src/MCPDemo.Api/MCPDemo.Api.csproj", "--configuration", "Release"]
       }
     }
   }
   ```

5. **Start Interacting**:
   Open your AI assistant and ask: *"List all product categories in the store."*

---

## 🛠 MCP Tool Catalog

| Category | Tool Name | Description | Returns |
|:---|:---|:---|:---|
| **Products** | `get_all_products` | List all products with optional pagination. | JSON Array (Product) |
| | `get_product_by_id` | Get a specific product by its numeric ID. | JSON Object (Product) |
| | `get_product_by_slug` | Get a specific product by its URL slug. | JSON Object (Product) |
| | `create_product` | Add a new product to the store. | JSON Object (Product) |
| | `update_product` | Modify an existing product. | JSON Object (Product) |
| | `delete_product` | Permanently remove a product. | Success Message |
| | `get_related_products_by_id` | Find products related to a specific ID. | JSON Array (Product) |
| | `get_related_products_by_slug`| Find products related to a specific slug. | JSON Array (Product) |
| **Categories** | `get_all_categories` | List all available product categories. | JSON Array (Category) |
| | `get_category_by_id` | Get a category by its numeric ID. | JSON Object (Category) |
| | `get_category_by_slug` | Get a category by its URL slug. | JSON Object (Category) |
| | `create_category` | Add a new category to the store. | JSON Object (Category) |
| | `update_category` | Modify an existing category. | JSON Object (Category) |
| | `delete_category` | Permanently remove a category. | Success Message |
| | `get_products_by_category` | List all products under a category. | JSON Array (Product) |
| **Search** | `search_products` | Flexible filters (title, price range, ID). | JSON Array (Product) |
| **Analytics** | `run_python_code` | Execute Python (Pandas/Numpy) in sandbox. | Console Output |
| **System** | `get_metrics` | View real-time tool usage and performance. | JSON Object (Metrics) |

---

## 🏗 Project Structure

```text
/
├── src/
│   ├── MCPDemo.Api              # MCP Server Host & Tool Definitions
│   ├── MCPDemo.Application      # Service Logic & Interfaces
│   ├── MCPDemo.Infrastructure   # API Client, Docker, Metrics
│   ├── MCPDemo.Domain           # Core Entities
│   └── MCPDemo.Shared           # Results & Exceptions
├── tests/
│   ├── MCPDemo.Infrastructure.Tests
│   └── MCPDemo.Integration.Tests
└── docs/                        # Implementation Plan & Specs
```

---

## 💻 Development Commands

- **Run all tests**:
  ```bash
  dotnet test MCPDemo.sln
  ```
- **Build with zero warnings**:
  ```bash
  dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true
  ```
- **Run the API directly**:
  ```bash
  dotnet run --project src/MCPDemo.Api/MCPDemo.Api.csproj
  ```

---

## 📄 License
MIT License. See [LICENSE](LICENSE) for details.
