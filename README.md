# Insurance Office — Multi-Company Assistant

A small conversational assistant that compares insurance quotes and coverage across multiple partner companies. It combines a lightweight React chat UI with a .NET API that orchestrates Model Context Protocol (MCP) calls to company-specific tools.

**Features**

- **Multi-company comparison:** Fetches quotes and coverage details from multiple company tools and presents concise side-by-side comparisons.
- **Conversational UI:** Chat-based interface in the `InsuranceOfficeUI` folder for natural-language interactions.
- **Tool orchestration via MCP:** Backend uses an MCP proxy to discover and call company tools, aggregating results server-side.
- **Schema-driven elicitation:** Required input (for example `clientAge`, `coverageType`, `assetValue`) is enforced by JSON schemas; when information is missing the assistant asks the user for it.
- **Language-aware responses:** The assistant answers in the same language used by the user.

**Technical note — elicitation via schemas**

- Elicitation (requesting missing user details) is implemented by defining JSON Schemas for tool inputs in the backend (see `InsuranceOfficeApi/Program.cs`). The API registers tools with these schemas so the language model knows which fields are required.
- When the user omits a required property (e.g., age for an auto quote), the model generates a clarification request (an elicitation question) to collect that field before calling the company tools.
- Schema examples are defined in `InsuranceOfficeApi/Program.cs` (see the `quoteSchema` and `coverageSchema` definitions) and the system prompt in the same file directs how the assistant should behave.

**Quick start**

1. Build the whole solution (recommended first step):

```powershell
dotnet build insurance-office.sln
```

2. Start the frontend and all services from the UI folder:

```bash
cd InsuranceOfficeUI
npm install
npm run start:all
```

This project includes an `npm` script `start:all` that launches the UI and any configured dev helpers to run the backend and proxy. If you prefer to run services individually, see the alternate commands below.

Alternate (run services individually):

- Start API (backend):

```bash
dotnet run --project InsuranceOfficeApi/InsuranceOfficeApi.csproj
```

- Start UI (dev server only):

```bash
cd InsuranceOfficeUI
npm run dev
```

Other backend services (run individually):

- MCP proxy service:

```bash
dotnet run --project McpProxy/McpProxy.Api/McpProxy.Api.csproj
```

- Communication MCP server:

```bash
dotnet run --project CommunicationMcpServer/CommunicationMcp.Api/CommunicationMcp.Api.csproj
```

- Insurance proxy service:

```bash
dotnet run --project InsuranceProxy/InsuranceProxy.csproj
```

- Company MCP servers (run one or all to simulate companies):

```bash
dotnet run --project TheLionMcpServer/TheLionMcpServer.csproj
dotnet run --project TheBlueCompanyMcpServer/TheBlueCompanyMcpServer.csproj
dotnet run --project TheThreeLinesMcpServer/TheThreeLinesMcpServer.csproj
```

3. Open the UI in your browser (Vite will show the dev URL, often `http://localhost:5173`). The UI communicates with the API at `http://localhost:5100` by default.

**Build recommendation**

- **Why:** `dotnet run` will build projects automatically, but running an explicit build first ensures the whole solution compiles and surfaces compilation or dependency errors early.

  **Where to adjust elicitation behavior**

- To change phrasing, language constraints, or when/how the assistant elicits missing fields, edit the system prompt and schema usage in `InsuranceOfficeApi/Program.cs`.
- To add or change required fields for quotes, update the JSON schema objects (`quoteSchema`, `coverageSchema`) in the same file.

**Development & tests**

- Run server-side tests:

```bash
dotnet test
```

- Run UI linting and type checks from `InsuranceOfficeUI` as configured in that package.

**Files and folders of interest**

- `InsuranceOfficeApi/Program.cs`: API entrypoint, tool discovery, schema registration, and system prompt.
- `InsuranceOfficeUI/`: React-based chat UI and suggested queries.
- `InsuranceProxy/` and MCP server folders: company tool implementations and proxy configuration.
