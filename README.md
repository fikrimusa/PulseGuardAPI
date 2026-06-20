# PulseGuard API

PulseGuard API is an ASP.NET Core Web API portfolio project for monitoring website and API health endpoints. Its intended responsibility is to record uptime history and identify repeated check failures so they can become actionable alerts.

> **Current status:** Foundation stage. This repository currently provides a health endpoint and Swagger UI only. All monitoring, persistence, and alerting capabilities below are planned; they are not implemented yet.

## Project overview

PulseGuard API will provide a backend foundation for defining health monitors, executing checks, storing their outcomes, and exposing a service's current and historical status. The project is deliberately backend-first, with a focus on API design, maintainable structure, and operational concerns.

## Tech stack

- **Framework:** ASP.NET Core Web API (.NET 8)
- **Language:** C#
- **API documentation:** Swagger / OpenAPI via Swashbuckle
- **Architecture:** Controller, service, repository, data, model, DTO, and configuration layers

The following are intentionally not part of the current implementation: authentication, PostgreSQL, Redis, Docker, background workers, and alert delivery.

## Current API

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/health` | Returns the API status and the current UTC timestamp. |

Example response:

```json
{
  "status": "Healthy",
  "timestampUtc": "2026-06-20T12:34:56.789Z"
}
```

Swagger UI is available at `/swagger` while the API is running.

## Planned features

### MVP

- Create and manage HTTP health monitors.
- Configure endpoint URLs, timeouts, expected HTTP status codes, and check intervals.
- Execute scheduled checks and retain uptime and response-time history.
- Expose current monitor status and recent check results through REST endpoints.
- Detect repeated failures and create alert records.

### Future features

- Authentication and user-owned monitors.
- PostgreSQL persistence and schema migrations.
- Background workers for scheduled checks.
- Alert delivery through email, webhooks, Slack, or Discord.
- Redis-backed caching or job coordination where justified.
- Docker-based local and deployment environments.
- Uptime reporting, performance summaries, teams, and role-based access.

## How to run locally

### Prerequisites

- .NET 8 SDK

### Run the API

```bash
dotnet restore
dotnet build
dotnet run
```

Open the following URLs after the server starts:

- Swagger UI: `http://localhost:5054/swagger`
- Health endpoint: `http://localhost:5054/api/health`

To call the health endpoint from a terminal:

```bash
curl http://localhost:5054/api/health
```

## Project roadmap

- [x] Create ASP.NET Core Web API foundation and health endpoint.
- [x] Enable Swagger / OpenAPI documentation.
- [ ] Define monitor, check-result, and alert domain models.
- [ ] Add a persistence layer and migrations.
- [ ] Implement monitor-management endpoints.
- [ ] Add scheduled health checks and uptime history.
- [ ] Detect repeated failures and create alert records.
- [ ] Add authentication, alert delivery, testing, and deployment tooling.

## Portfolio purpose

PulseGuard API is being built to demonstrate backend engineering skills relevant to Backend Engineer, Software Engineer, and C# .NET Developer roles. The planned work covers REST API design, layered architecture, background processing, data modelling, reliability-oriented workflows, testing, and production-minded operational design.
