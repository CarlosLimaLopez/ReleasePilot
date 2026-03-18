# ReleasePilot

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/)

## Getting Started

### 1. Start Infrastructure Services

Run the following command from the solution root to start the required dependencies (PostgreSQL, RabbitMQ, etc.):

```bash
docker compose up -d
```

### 2. Apply Database Migrations (First Time Only)

If this is the first time running the application, apply the EF Core migrations to create the database schema:

```bash
dotnet ef database update --project src/ReleasePilot.Infrastructure --startup-project src/ReleasePilot.Api
```

### 3. Run the Application

Open `ReleasePilot.sln` in Visual Studio, set **ReleasePilot.Api** as the startup project, and press **F5** to run.

The API will be available at the URL shown in the console output. Swagger UI is enabled by default for exploring the available endpoints.
