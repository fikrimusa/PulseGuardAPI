# AWS EC2 deployment plan

This guide describes a manual, Docker Compose-based deployment of PulseGuard API to a single Amazon EC2 instance. It is intended for a portfolio demonstration and does not add deployment automation, infrastructure as code, HTTPS, or managed AWS secrets.

## Target architecture

```text
Internet
   |
EC2 security group (SSH and temporary API access)
   |
EC2 instance
   |- PulseGuard API container (port 8080)
   `- PostgreSQL container (private to the instance)
```

The initial deployment keeps PostgreSQL in a Docker container on the same EC2 instance. This keeps the setup small and mirrors local Docker Compose development. For a more durable production database, move PostgreSQL to Amazon RDS later.

## AWS resources

Create the following resources manually in the AWS Console:

1. **EC2 instance** — an Ubuntu LTS instance is suitable for this guide. Choose an instance size appropriate to the expected load; a small instance is enough for a portfolio deployment.
2. **Security group** — attach it to the EC2 instance and use these inbound rules:

   | Port | Source | Purpose |
   | --- | --- | --- |
   | `22` (SSH) | Your current public IP only | Administrative access |
   | `8080` (TCP) | Your current public IP only, temporarily | Test Swagger and the health endpoint before HTTPS exists |
   | `80` / `443` | Add later when a reverse proxy and HTTPS are configured | Public HTTP/HTTPS |

   Do **not** add inbound rules for PostgreSQL ports `5432` or `5433`. The API reaches the database through the private Docker Compose network.

3. **Optional future RDS PostgreSQL instance** — do not create this for the first deployment. When introduced, place RDS in private subnets and allow database port `5432` only from the EC2 instance's security group.

## Before deployment

- Create an EC2 key pair and keep the downloaded `.pem` file private.
- Ensure the repository is public, or configure a deploy key before cloning a private repository.
- Install Docker Engine and the Docker Compose plugin on the EC2 instance. Follow Docker's current [Ubuntu installation guide](https://docs.docker.com/engine/install/ubuntu/), then confirm:

  ```bash
  docker --version
  docker compose version
  ```

- Install Git if it is not already present:

  ```bash
  sudo apt-get update
  sudo apt-get install -y git
  ```

## Deployment environment variables

Create the `.env` file on the EC2 instance only. It is ignored by Git and must never be committed.

| Variable | Example / purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ConnectionStrings__PulseGuardDatabase` | `Host=postgres;Port=5432;Database=pulseguard;Username=pulseguard;Password=<strong-password>`; the current Compose file supplies this to the API from the PostgreSQL variables below. Use this value directly if the API is later deployed without the bundled database container. |
| `Jwt__Key` | A strong random signing key. The Compose `.env` name is `JWT_KEY`. |
| `Jwt__Issuer` | For example, `PulseGuard.Api`; the Compose `.env` name is `JWT_ISSUER`. |
| `Jwt__Audience` | For example, `PulseGuard.Api.Client`; the Compose `.env` name is `JWT_AUDIENCE`. |
| `HealthCheckWorker__PollingIntervalSeconds` | For example, `30`; the Compose `.env` name is `HEALTH_CHECK_WORKER_POLLING_INTERVAL_SECONDS`. |
| `POSTGRES_DB` | Database name used by the PostgreSQL container, for example `pulseguard`. |
| `POSTGRES_USER` | Database user used by the PostgreSQL container. |
| `POSTGRES_PASSWORD` | Strong PostgreSQL password. |

Generate secrets locally on the EC2 instance with:

```bash
openssl rand -base64 48
```

## Deploy to EC2

### 1. Connect to the instance

From your computer, restrict the key file and connect using the EC2 public IPv4 address or DNS name:

```bash
chmod 400 pulseguard-ec2.pem
ssh -i pulseguard-ec2.pem ubuntu@<ec2-public-ip-or-dns>
```

### 2. Install and enable Docker

Follow the Docker installation link above. After installation, allow the Ubuntu user to run Docker without `sudo`, then reconnect:

```bash
sudo usermod -aG docker "$USER"
exit
```

SSH in again and confirm `docker ps` works.

### 3. Clone the repository

```bash
git clone https://github.com/fikrimusa/PulseGuardAPI.git
cd PulseGuardAPI
```

### 4. Configure `.env`

Create the file with a terminal editor such as `nano .env` and add real values. Do not use the example passwords in the repository.

```dotenv
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

POSTGRES_DB=pulseguard
POSTGRES_USER=pulseguard
POSTGRES_PASSWORD=<generate-a-strong-database-password>

JWT_KEY=<paste-openssl-random-output-here>
JWT_ISSUER=PulseGuard.Api
JWT_AUDIENCE=PulseGuard.Api.Client

HEALTH_CHECK_WORKER_POLLING_INTERVAL_SECONDS=30
```

The current `docker-compose.yml` builds `ConnectionStrings__PulseGuardDatabase` automatically with `Host=postgres`, so the API reaches the database container by its Compose service name. Do not use `localhost` for the container-to-container database host.

### 5. Start the containers

```bash
docker compose up -d --build
docker compose ps
docker compose logs -f api
```

Use `Ctrl+C` to stop following logs; it does not stop the containers. For a server that only has the legacy Compose command, replace `docker compose` with `docker-compose`.

### 6. Apply Entity Framework migrations

The repository includes migrations, but a new PostgreSQL database needs them applied before the API can use its tables.

Install the .NET 8 SDK and EF Core CLI on the EC2 instance if they are not already available, then run this from the repository root:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
export PATH="$PATH:$HOME/.dotnet/tools"

ConnectionStrings__PulseGuardDatabase='Host=localhost;Port=5433;Database=pulseguard;Username=pulseguard;Password=<your-postgres-password>' \
  dotnet ef database update
```

The existing Compose configuration exposes PostgreSQL on host port `5433` for this administrative migration command. Do not open that port in the EC2 security group. A later production hardening change can remove this host port entirely and run migrations from a controlled job on the Docker network.

Restart the API after migrations so the health-check worker begins against the initialized schema:

```bash
docker compose restart api
```

### 7. Verify the deployment

From the EC2 instance:

```bash
curl http://localhost:8080/api/health
```

From your own computer, while the temporary `8080` security-group rule allows your IP:

```text
http://<ec2-public-ip-or-dns>:8080/swagger
http://<ec2-public-ip-or-dns>:8080/api/health
```

After verification, remove public port `8080` access unless it is intentionally needed. Use a reverse proxy and HTTPS before making the API generally public.

## Production notes and next steps

- Keep PostgreSQL private: no public security-group rule for `5432` or `5433`.
- Use a unique, high-entropy JWT signing key. Rotate it if it is exposed; changing it invalidates existing tokens.
- Restrict SSH and temporary API access to known IP addresses. Avoid `0.0.0.0/0` for SSH.
- The API currently serves HTTP directly on port `8080`. Add Nginx, Caddy, or another reverse proxy with TLS certificates before exposing it publicly on ports `80` and `443`.
- Back up the Docker PostgreSQL volume before upgrades. A container database is appropriate for the first portfolio deployment but is not the preferred durable production design.
- In a future RDS migration, update `ConnectionStrings__PulseGuardDatabase` to the RDS endpoint, remove the PostgreSQL container, and permit RDS access only from the EC2 security group.

## Useful operations

```bash
# View running containers
docker compose ps

# Follow application logs
docker compose logs -f api

# Stop containers while keeping the database volume
docker compose down

# Rebuild and start after pulling changes
git pull
docker compose up -d --build
```
