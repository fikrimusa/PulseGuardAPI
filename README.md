# PulseGuard API

[![Continuous Integration](https://github.com/fikrimusa/PulseGuardAPI/actions/workflows/ci.yml/badge.svg)](https://github.com/fikrimusa/PulseGuardAPI/actions/workflows/ci.yml)

PulseGuard API is an ASP.NET Core Web API portfolio project for monitoring website and API health endpoints. Its intended responsibility is to record uptime history and identify repeated check failures so they can become actionable alerts.

> **Current status:** MVP backend stage. The API provides a health endpoint, Swagger UI, PostgreSQL-backed monitor CRUD, JWT authentication, scheduled health checks, persisted alerts, a user-scoped dashboard summary, and Docker-based local development. External alert delivery is not implemented yet.

## Project overview

PulseGuard API provides a backend foundation for defining health monitors, executing checks, storing their outcomes, and exposing a service's current and historical status.

## Tech stack

- **Framework:** ASP.NET Core Web API (.NET 8)
- **Language:** C#
- **API documentation:** Swagger / OpenAPI via Swashbuckle
- **Persistence:** Entity Framework Core 8 with the Npgsql PostgreSQL provider
- **Authentication:** JWT bearer tokens with ASP.NET Core password hashing
- **Health checks:** Hosted background service using `HttpClientFactory`
- **Architecture:** Controller, service, repository, data, model, DTO, and configuration layers

The following are intentionally not part of the current implementation: Redis and external alert delivery.

## Current API

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/health` | Returns the API status and the current UTC timestamp. |
| `POST` | `/api/auth/register` | Creates a user account and returns a JWT. |
| `POST` | `/api/auth/login` | Authenticates a user and returns a JWT. |
| `POST` | `/api/monitors` | Creates a persisted monitor. |
| `GET` | `/api/monitors` | Lists persisted monitors. |
| `GET` | `/api/monitors/{id}` | Retrieves a monitor by ID. |
| `PUT` | `/api/monitors/{id}` | Replaces a monitor's editable settings. |
| `DELETE` | `/api/monitors/{id}` | Deletes a monitor. |
| `GET` | `/api/monitors/{id}/checks` | Retrieves health-check history for an owned monitor. |
| `GET` | `/api/alerts` | Lists alerts for monitors owned by the authenticated user. |
| `GET` | `/api/alerts/{id}` | Retrieves an owned alert. |
| `PUT` | `/api/alerts/{id}/acknowledge` | Acknowledges an open owned alert. |
| `GET` | `/api/dashboard/summary` | Returns the authenticated user's monitor, alert, and recent-check summary. |

Example response:

```json
{
  "status": "Healthy",
  "timestampUtc": "2026-06-20T12:34:56.789Z"
}
```

Swagger UI is available at `/swagger` while the API is running.

## Deployment documentation

See the [AWS EC2 deployment plan](docs/aws-deployment.md) for a beginner-friendly, manual Docker Compose deployment guide. It documents the required AWS resources, environment variables, security-group rules, migrations, and a future RDS path; it does not add deployment automation.

## Features

### Implemented MVP features

- Register users and issue JWT bearer tokens using hashed passwords.
- Create, list, retrieve, update, and delete user-owned monitors persisted in PostgreSQL.
- Run due checks for enabled monitors and persist status, latency, and failure details.
- Open, acknowledge, and automatically resolve monitor failure alerts.
- Summarise monitor health, alert counts, and recent activity for the authenticated user.
- Configure a monitor name, endpoint URL, check interval, and active state.

### Future features

- Alert delivery through email, webhooks, Slack, or Discord.
- Redis-backed caching or job coordination where justified.
- Docker-based local and deployment environments.
- Uptime reporting, performance summaries, teams, and role-based access.

## How to run locally

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14 or later

### Configure PostgreSQL

Create a local database and user:

```bash
sudo -u postgres psql
CREATE USER pulseguard_user WITH PASSWORD 'replace-with-a-local-password';
CREATE DATABASE pulseguard OWNER pulseguard_user;
\q
```

Set the connection string for the current terminal. This overrides the placeholder value in `appsettings.json` and avoids committing a real password:

```bash
export ConnectionStrings__PulseGuardDatabase='Host=localhost;Port=5432;Database=pulseguard;Username=pulseguard_user;Password=replace-with-a-local-password'
```

On Ubuntu, PostgreSQL's default local peer authentication can be used instead of a password. After creating a matching PostgreSQL role and database, use the Unix socket connection string:

```bash
sudo -u postgres createuser --login "$USER"
sudo -u postgres createdb --owner="$USER" pulseguard
export ConnectionStrings__PulseGuardDatabase="Host=/var/run/postgresql;Port=5432;Database=pulseguard;Username=$USER"
```

### Configure JWT

`appsettings.json` contains a development-only JWT key so the project can run locally. Override it for any environment that is shared or deployed:

```bash
export Jwt__Key="$(openssl rand -base64 48)"
```

Do not commit a real JWT signing key.

Install the EF Core CLI tool once (the major version must match EF Core 8):

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

Apply the committed database migration:

```bash
dotnet ef database update
```

Create a new migration after changing an EF Core model:

```bash
dotnet ef migrations add DescriptiveMigrationName
```

### Run with Docker

#### Prerequisites

- Docker Engine with either the Docker Compose plugin (`docker compose`) or the legacy CLI (`docker-compose`)

Start the API and PostgreSQL containers:

```bash
docker compose up --build
```

For Docker Compose 1.x, use the equivalent command:

```bash
docker-compose up --build
```

The API uses the Compose service name `postgres` to connect to PostgreSQL. PostgreSQL is exposed on host port `5433` to avoid conflicts with a local PostgreSQL instance on port `5432`.

Apply EF Core migrations from the host after the containers are running:

```bash
ConnectionStrings__PulseGuardDatabase='Host=localhost;Port=5433;Database=pulseguard;Username=pulseguard;Password=pulseguard_dev_password' dotnet ef database update
```

Open Swagger at `http://localhost:8080/swagger`.

Stop the containers:

```bash
docker compose down
```

For Docker Compose 1.x:

```bash
docker-compose down
```

To also delete the Docker PostgreSQL volume and all local container data:

```bash
docker compose down --volumes
```

For Docker Compose 1.x:

```bash
docker-compose down -v
```

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

### Run automated tests

Run the backend test suite from the repository root:

```bash
dotnet test PulseGuard.sln
```

The tests use an in-memory database and cover authentication, monitor ownership, alert state transitions, dashboard aggregation, and basic API behavior without requiring PostgreSQL or Docker.

The suite currently validates:

- `GET /api/health` returns a successful response.
- Registering and logging in a user returns a JWT access token.
- Protected monitor endpoints reject requests without a JWT token.
- A user cannot retrieve a monitor owned by another user.
- Three consecutive failed checks create one `OPEN` alert; a later successful check resolves it.
- Acknowledging an alert changes its status to `ACKNOWLEDGED` and records the acknowledgement time.
- Dashboard totals and average response time are calculated from the current user's monitors, checks, and alerts only.

For the name and result of each test, run:

```bash
dotnet test PulseGuard.sln --logger "console;verbosity=detailed"
```

### Authenticate in Swagger

1. Open `http://localhost:5054/swagger`.
2. Call `POST /api/auth/register` with an email and a password of at least eight characters.
3. Copy `accessToken` from the response.
4. Click **Authorize**, enter the token, and confirm.
5. Call the protected `/api/monitors` endpoints. Each user can access only their own monitors.

### Health checks

The background worker starts with the API. It polls every 10 seconds by default and performs a check only when an enabled monitor's `checkIntervalSeconds` is due. Each check sends an HTTP `GET` request and compares its response status with `expectedStatusCode`.

Monitor create and update requests support these health-check settings:

```json
{
  "checkIntervalSeconds": 60,
  "timeoutSeconds": 10,
  "expectedStatusCode": 200,
  "isActive": true
}
```

Set `HealthCheckWorker__PollingIntervalSeconds` to change the local polling frequency. This does not override the per-monitor `checkIntervalSeconds` setting.

### Alerts

An alert opens after **three consecutive failed checks** for the same monitor. Only one active alert can exist per monitor: additional failures update its `failureCount` instead of creating duplicates. A successful check automatically changes an `OPEN` or `ACKNOWLEDGED` alert to `RESOLVED`.

Alert statuses are `OPEN`, `ACKNOWLEDGED`, and `RESOLVED`. Alert endpoints return only alerts that belong to the authenticated user's monitors.

### Dashboard summary

`GET /api/dashboard/summary` returns counts for the authenticated user's monitors and alerts, the latest status of each monitor, average response time from the latest checks, and up to ten recent alerts and checks. A monitor is `UNKNOWN` until it has a check result.

## Project roadmap

- [x] Create ASP.NET Core Web API foundation and health endpoint.
- [x] Enable Swagger / OpenAPI documentation.
- [x] Define the initial monitor model and in-memory CRUD endpoints.
- [x] Add PostgreSQL persistence and the initial EF Core migration.
- [x] Add JWT authentication and user-owned monitor access.
- [x] Add scheduled health checks and persisted check history.
- [x] Add consecutive-failure alerts with acknowledgement and automatic resolution.
- [x] Add a user-scoped dashboard summary API.
- [x] Add Docker support for local development.
- [x] Add automated tests for services and API endpoints.
- [x] Add GitHub Actions CI pipeline.
- [x] Add AWS deployment documentation.

## Portfolio purpose

PulseGuard API demonstrates backend engineering skills relevant to Backend Engineer, Software Engineer, and C# .NET Developer roles. The project covers REST API design, layered architecture, background processing, data modelling, reliability-oriented workflows, testing, and production-minded operational design.
