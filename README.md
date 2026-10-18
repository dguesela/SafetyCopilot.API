# SafetyCopilot API

This repository contains the backend API for the SafetyCopilot research prototype. The API extracts requirements from PDF documents, stores them, allows human classification, and can run experimental AI classification using a user-specific OpenAI API key. The front-end web UI is maintained separately. Starting this backend alone does not provide a working end-to-end system.

> Supported target framework: `net8.0` (requires .NET 8 SDK)

## Prerequisites

- .NET 8 SDK: [Download .NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server or LocalDB (or any SQL Server-compatible instance)

## Quick setup

1. Open a terminal and navigate to the API project folder:

   ```powershell
   cd C:\Repository\SafetyCopilot.API\SafetyCopilot.API
   ```

2. Configure the database connection

   Create an `appsettings.Development.json` next to `appsettings.json` (or set the environment variable `ConnectionStrings__DefaultConnection`) with a `DefaultConnection` entry.

   Example `appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=[SQL SERVER GOES HERE]\\mssqllocaldb;Database=SafetyCopilotDb;Trusted_Connection=True;"
     }
   }
   ```

   Or set it at the shell (PowerShell):

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Server=[SQL SERVER GOES HERE]\\mssqllocaldb;Database=SafetyCopilotDb;Trusted_Connection=True;"
   ```

3. Run the API
   - **Visual Studio 2022**: Open the folder or project, set `SafetyCopilot.API` as the startup project, and press F5 (or use **Debug > Start Without Debugging**).
   - **CLI**:

   ```powershell
   dotnet run
   ```

   Kestrel will log the exact URLs (HTTP/HTTPS) the app listens on. You can also set `ASPNETCORE_URLS` to force a specific URL.

4. Open Swagger (Development only)

   When running in Development, Swagger UI is available at `/swagger` (e.g. `https://localhost:5001/swagger`).

## Important API endpoints (examples)

Note: This API intentionally omits authentication middleware for the simplified research prototype. Several endpoints accept `userId` in the body or query string.

- **Register a new user**

  ```bash
  curl -sS -X POST "https://localhost:5001/api/auth/register" \
    -H "Content-Type: application/json" \
    -d '{"email":"alice@example.com","password":"Password123!","displayName":"Alice"}'
  ```

- **Login**

  ```bash
  curl -sS -X POST "https://localhost:5001/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"alice@example.com","password":"Password123!"}'
  ```

- **Create a project**

  ```bash
  curl -sS -X POST "https://localhost:5001/api/projects" \
    -H "Content-Type: application/json" \
    -d '{"userId":"<USER_ID>","name":"My Study Project","description":"Test project"}'
  ```

- **Upload a PDF requirements document (multipart form)**

  ```bash
  curl -X POST "https://localhost:5001/api/projects/<PROJECT_ID>/documents" \
    -F "file=@/path/to/requirements.pdf"
  ```

- **Extract requirements from an uploaded document**

  ```bash
  curl -sS -X POST "https://localhost:5001/api/documents/<DOCUMENT_ID>/extract-requirements"
  ```

- **List requirements for a project**

  ```bash
  curl -sS "https://localhost:5001/api/projects/<PROJECT_ID>/requirements"
  ```

- **Save OpenAI API key for a user (stored encrypted)**

  ```bash
  curl -sS -X PUT "https://localhost:5001/api/profile/<USER_ID>/api-key" \
    -H "Content-Type: application/json" \
    -d '{"apiKey":"sk-XXXX"}'
  ```

- **Run AI classification for a project**

  ```bash
  curl -sS -X POST "https://localhost:5001/api/projects/<PROJECT_ID>/ai-classification" \
    -H "Content-Type: application/json" \
    -d '{"userId":"<USER_ID>"}'
  ```

Adjust host/port to the URLs reported by the app when it starts.

## CORS

The application allows CORS from `http://localhost:5173` and `http://localhost:5174` (React dev servers). If your frontend uses a different port, update the CORS policy in `Program.cs`.

## Notes about OpenAI keys

- OpenAI API keys are stored per-user in `tblUser.OpenAiApiKeyEncrypted`; they are encrypted using ASP.NET Core Data Protection.
- Use the profile endpoints to save/remove a key (see example above).
