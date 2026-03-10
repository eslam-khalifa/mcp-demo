# 🔌 MCP: The Backend Engineer's Guide

A concise, point-by-point reference for understanding the Model Context Protocol (MCP) from a system architecture and development perspective.

### 🧮 Simple Example: Calculator Tool

**1. AI Model (Standalone LLM)**
*   **Behavior:** Relies on internal training patterns.
*   **Process:** When asked "What is 123 * 456?", it tries to predict the next token based on its training (like memorizing $2 + 3 = 5$).
*   **Risk:** Can "hallucinate" or provide incorrect results for complex or very large numbers.

**2. AI Agent with MCP (AI Model + Calculator Tool)**
*   **Behavior:** Delegates the calculation to a reliable external program.
*   **Process:** The model recognizes a mathematical request and triggers a **Calculator Tool** (MCP Tool) to compute the exact sum/product.
*   **Benefit:** Guaranteed precision by using a deterministic tool instead of probabilistic prediction.

---

### 🛒 Real-World Example: E-commerce Assistant (Scenario: Platzi Fake Store API)

**1. AI Model (Standalone LLM)**
*   **Status:** A "Static Knowledge Base" that suggests products based on training data.
*   **Interaction:** User asks "What's a good laptop under $500?" and the model suggests one based on general knowledge.
*   **Limitations:** No access to **Platzi API** for real-time stock, cannot check current prices, and can't remember past user orders.
*   **Analogy:** A knowledgeable salesperson who has no connection to the store's inventory system or cash register.

**2. AI Agent with MCP (AI Model + Context + Platzi API)**
*   **Status:** An "Action-Oriented Agent" with programmatic environment access.
*   **Interaction:** User asks same question; the Agent calls **Platzi API endpoints** to fetch live product lists, checks stock, and suggests options.
*   **Capabilities:** Once the user confirms, it executes the order via API, remembers preference (e.g., "likes SSDs"), and tracks shipments automatically.
*   **Analogy:** A digital personal shopper who can access the warehouse, apply your loyalty points, and complete the purchase for you.

---

### What is MCP?

MCP, developed by Anthropic, is an **open standard protocol** that connects AI models to external data and tools.

*   **Universal Bridge**: One protocol to link any AI model with any data source.
*   **Interoperable**: Build a tool once; use it across Claude, IDEs, and custom apps.
*   **Client-Server**: Standardizes communication between the AI (Client) and the data (Server).
*   **Live Context**: Gives models real-time access to files, databases, and APIs.

### MCP Components

* ![alt text](image.png)
*   **Host**: The application (e.g., Claude Desktop, IDE) where you interact with the AI.
*   **Client**: The internal bridge within the Host that maintains the connection to servers. without MCP client the AI model will not be able to access the servers.
*   **Server**: exposes tools like API, DB, files, etc... to the LLM.

### How MCP Works
* ![alt text](image-1.png)
1. You ask a question to the MCP host.
2. MCP host asks MCP server for the available tools
3. MCP server returns the available tools to MCP host.
4. MCP host sends the available tools + your question to the LLM
5. LLM decides to use a tool and sends the required tool to MCP host
6. MCP host calls the the MCP server for the tool result
7. MCP server will use the availabe resources with him like DB, API, files and etc...
8. MCP server returns the tool result back to the MCP host.
9. MCP host sends the tool result to the LLM
10. LLM uses the tool result to answer your question
11. MCP host sends the answer to you

### Why I need MCP instead of API calls?

- without MCP:
* ![alt text](image-5.png)
- each service has its own API and you need to create a custom integration for each service

- with MCP:
* ![alt text](image-4.png)
- each service has its own MCP server and you can use the same MCP host to connect to all of them

### How to connect MCP server to MCP host (Claude, VS Code, etc...)?

* ![alt text](image-3.png)
- through the configuration file for each each MCP host

### Agent should focus on a specific domain to avoid hallucination

* ![alt text](image-6.png)
* ![alt text](image-7.png)

### MCP usecases

* ![alt text](image-8.png)
* ![alt text](image-9.png)
  - in case of developing an API and I need to make my AI agent to have an access to my database (mongoDb, mysql, etc...) and make some actions like create, update, delete, etc... 
