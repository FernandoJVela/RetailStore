# Docker: Theory & Practical Guide for RetailStore

This document covers all Docker theory a software developer/architect must know, then applies every concept directly to the RetailStore project (ASP.NET Core 10 backend + React/Nginx frontend + SQL Server). It follows the same structure as `KUBERNETES.md` so the two documents can be read in sequence — Docker first, then Kubernetes when scaling demands it.

---

## Table of Contents

1. [Why Docker? (And When Not to Use It)](#1-why-docker-and-when-not-to-use-it)
2. [Core Concepts](#2-core-concepts)
3. [Dockerfile: Building Images Layer by Layer](#3-dockerfile-building-images-layer-by-layer)
4. [Multi-Stage Builds](#4-multi-stage-builds)
5. [Docker Compose: Orchestrating Services](#5-docker-compose-orchestrating-services)
6. [Networking](#6-networking)
7. [Volumes & Data Persistence](#7-volumes--data-persistence)
8. [Configuration & Secrets](#8-configuration--secrets)
9. [Health Checks & Service Dependencies](#9-health-checks--service-dependencies)
10. [Development Workflow](#10-development-workflow)
11. [Production Workflow](#11-production-workflow)
12. [Testing with Docker](#12-testing-with-docker)
13. [What Was Actually Built](#13-what-was-actually-built)
14. [How the App Benefits from This](#14-how-the-app-benefits-from-this)
15. [Running in Development (Local)](#15-running-in-development-local)
16. [Deploying to Production](#16-deploying-to-production)
17. [Day-to-Day Operations Reference](#17-day-to-day-operations-reference)
18. [Docker Compose → Kubernetes Migration Path](#18-docker-compose--kubernetes-migration-path)

---

## 1. Why Docker? (And When Not to Use It)

### The problem Docker solves

Without Docker, setting up RetailStore on a new machine requires:

1. Install .NET 10 SDK (specific version)
2. Install Node.js 22 (specific version)
3. Install SQL Server and configure a LocalDB instance
4. Set the correct connection string in `appsettings.json`
5. Set the correct JWT secret
6. Run `dotnet restore`, `npm install`, `dotnet run`, `npm run dev`

If any step uses a different version or configuration, the app behaves differently — or does not run at all. This is the famous "works on my machine" problem.

Docker solves this by packaging the application together with its entire runtime environment (OS libraries, language runtime, configuration) into a single, portable artifact called an **image**. Running that image produces a **container** that behaves identically on any machine that has Docker installed.

### What Docker provides for RetailStore

| Problem (without Docker) | Solution (with Docker) |
|---|---|
| Different .NET versions between developer machines | SDK version pinned in `FROM mcr.microsoft.com/dotnet/sdk:10.0` |
| "Install SQL Server" instructions in a README | `mcr.microsoft.com/mssql/server:2022-latest` starts in seconds |
| LocalDB only works on Windows | SQL Server Linux container runs on macOS, Linux, Windows |
| Manual connection string setup | Environment variable injected at container start |
| Frontend proxy manually configured | `VITE_API_PROXY_TARGET` env var switches automatically |
| Onboarding a new developer takes hours | `docker compose up --build` — done |

### When Docker Compose is enough

- **Single-machine deployments** — one server, low traffic
- **Development environments** — every developer runs the same stack
- **CI/CD test pipelines** — isolated, reproducible test runs
- **Small teams (< 10)** — operational overhead of Kubernetes is not justified

RetailStore uses Docker Compose for development and production on a single server. When you need multi-node clustering, auto-scaling, and zero-downtime rolling deploys, see `KUBERNETES.md`.

---

## 2. Core Concepts

### 2.1 Image vs Container

An **image** is a read-only, immutable snapshot of a filesystem plus a startup command. Think of it as a class definition.

A **container** is a running instance of an image — an isolated process with its own filesystem, network interface, and process tree. Think of it as an object instantiated from the class.

```
Image (read-only)        Container (running instance)
───────────────────      ──────────────────────────────
retailstore-backend  →   container_id: a3f2c1d
    .NET 10 runtime      PORT 8080 listening
    compiled DLLs        connected to retailstore_net
    appsettings.json     environment vars injected
```

Multiple containers can run from the same image simultaneously. Each gets its own writable layer on top.

### 2.2 Layers and the Build Cache

Every instruction in a Dockerfile creates a **layer** — an immutable filesystem diff. Docker stacks layers to form the complete image.

```
Layer 4  COPY --from=publish /app/publish .     ← DLLs + config files
Layer 3  RUN dotnet publish ...                 ← compiled output
Layer 2  COPY src/ src/ && COPY tests/ tests/  ← source code
Layer 1  RUN dotnet restore ...                 ← NuGet packages (~500 MB)
Layer 0  FROM mcr.microsoft.com/dotnet/sdk:10.0 ← base OS + .NET SDK
```

**Layer caching** is the most important performance mechanism in Docker. If a layer's inputs have not changed since the last build, Docker reuses the cached result — skipping execution entirely.

This is why the RetailStore backend Dockerfile copies `.csproj` files and runs `dotnet restore` **before** copying the source code. `dotnet restore` downloads ~500 MB of NuGet packages. If only a `.cs` file changed, Docker reuses the restore layer from cache and only re-runs the layers that follow it.

```dockerfile
# Layer 1: restore — reused from cache unless a .csproj changes
COPY src/RetailStore.Api/RetailStore.Api.csproj  src/RetailStore.Api/
RUN dotnet restore tests/RetailStore.Tests/RetailStore.Tests.csproj

# Layer 2: source — invalidated on every code change (fast to run)
COPY src/ src/
COPY tests/ tests/
```

### 2.3 Container Registry

A **registry** stores and distributes images. The default is [Docker Hub](https://hub.docker.com/). Others:

| Registry | Use case |
|---|---|
| Docker Hub | Public images (official SQL Server, .NET, Nginx, Node.js) |
| GitHub Container Registry (`ghcr.io`) | Private images tied to a GitHub repo |
| Azure Container Registry (ACR) | Microsoft cloud deployments |
| Amazon ECR | AWS deployments |

RetailStore pulls base images from Docker Hub (`mcr.microsoft.com` is Microsoft's registry, hosted on Docker Hub infrastructure) and pushes its own built images to `ghcr.io` in the CI/CD pipeline.

### 2.4 The `.dockerignore` file

Like `.gitignore` but for the Docker build context. When you run `docker build`, Docker sends the entire build context (the directory) to the Docker daemon. A `.dockerignore` prevents large or sensitive directories from being included.

RetailStore has two:

**`/.dockerignore`** (backend build context):
```
.git
retail-store-frontend   ← excluded: separate build context
**/bin                  ← compiled output rebuilt inside Docker
**/obj
.env                    ← secrets must never enter an image
```

**`/retail-store-frontend/.dockerignore`**:
```
node_modules            ← Docker installs its own via npm ci
dist                    ← rebuilt inside Docker
e2e/.auth              ← browser auth state from Playwright
```

---

## 3. Dockerfile: Building Images Layer by Layer

### 3.1 Core instructions

| Instruction | What it does |
|---|---|
| `FROM image:tag` | Start from a base image. Every `FROM` begins a new stage. |
| `WORKDIR /path` | Set the working directory for subsequent instructions |
| `COPY src dest` | Copy files from the build context (your machine) into the image |
| `RUN command` | Execute a shell command and commit the result as a new layer |
| `ENV KEY=value` | Set an environment variable baked into the image |
| `EXPOSE port` | Document which port the container listens on (informational only) |
| `ENTRYPOINT ["cmd"]` | The command that runs when the container starts (exec form — no shell) |
| `CMD ["arg"]` | Default arguments to ENTRYPOINT, or the command itself if ENTRYPOINT is absent |
| `ARG name=default` | Build-time variable (not available at runtime) |

### 3.2 ENTRYPOINT vs CMD

```dockerfile
# ENTRYPOINT: the fixed executable — cannot be overridden without --entrypoint
ENTRYPOINT ["dotnet", "RetailStore.Api.dll"]

# CMD: default arguments — can be replaced on the command line
CMD ["--migrate"]

# Together: docker run image        → dotnet RetailStore.Api.dll --migrate
#           docker run image --help → dotnet RetailStore.Api.dll --help
```

The backend Dockerfile uses only `ENTRYPOINT` — the command is always `dotnet RetailStore.Api.dll`. The frontend dev stage uses only `CMD` — the Vite dev server startup is the default but can be overridden.

### 3.3 Exec form vs Shell form

```dockerfile
# Shell form — runs via /bin/sh -c, supports variable expansion
RUN dotnet restore $PROJECT_FILE

# Exec form — runs directly, no shell overhead, correct signal handling
ENTRYPOINT ["dotnet", "RetailStore.Api.dll"]
```

Always use exec form (`["cmd", "arg"]`) for `ENTRYPOINT` and `CMD`. Shell form wraps the process in `sh -c`, which means Docker sends `SIGTERM` to the shell, not to your application. This breaks graceful shutdown.

---

## 4. Multi-Stage Builds

### 4.1 Theory

A multi-stage build uses multiple `FROM` instructions in one Dockerfile. Each `FROM` starts a new **stage** with a fresh filesystem. You can then `COPY --from=<stage>` to pull specific files from a previous stage into the current one.

The **final image** is built from the last `FROM` instruction. All intermediate stages are discarded. This means:

- The production image does not contain the .NET SDK, Node.js, test frameworks, or source code
- The production image only contains the compiled output and its runtime
- Images are smaller, start faster, and have a smaller attack surface

### 4.2 Backend: four stages

**`/Dockerfile`**

```
┌─────────────────────────────────────────────────────────────┐
│ Stage: build  (mcr.microsoft.com/dotnet/sdk:10.0 ~900 MB)  │
│   • COPY .csproj files                                      │
│   • dotnet restore (NuGet packages cached as a layer)       │
│   • COPY all source code                                    │
└───────────────────────┬─────────────────────────────────────┘
                        │ inherited by
           ┌────────────┴────────────┐
           ▼                         ▼
┌──────────────────────┐  ┌──────────────────────────────────┐
│ Stage: test          │  │ Stage: publish                   │
│   • dotnet test      │  │   • dotnet publish -c Release    │
│   • exits 0 or 1     │  │   • output → /app/publish        │
└──────────────────────┘  └─────────────────┬────────────────┘
  Used by:                                  │ COPY --from=publish
  docker-compose.test.yml                   ▼
                              ┌──────────────────────────────┐
                              │ Stage: runtime  (~250 MB)     │
                              │ FROM dotnet/aspnet:10.0       │
                              │   • COPY compiled DLLs only  │
                              │   • No SDK, no source        │
                              │   • ENTRYPOINT dotnet ...    │
                              └──────────────────────────────┘
                                Used by: docker-compose.yml
                                         docker-compose.prod.yml
```

The `test` and `publish` stages both inherit from `build`. They are siblings — the result of one does not affect the other. Docker builds each on demand based on which `--target` is requested.

### 4.3 Frontend: five stages

**`/retail-store-frontend/Dockerfile`**

```
┌──────────────────────────────────────────────────┐
│ Stage: deps  (node:22-alpine)                    │
│   • COPY package*.json                           │
│   • npm ci (exact locked versions)               │
└──────────────┬───────────────────────────────────┘
               │ inherited by all stages below
    ┌──────────┼──────────┬──────────────┐
    ▼          ▼          ▼              ▼
┌────────┐ ┌────────┐ ┌────────┐  ┌────────────────────┐
│  dev   │ │  test  │ │ build  │  │      runtime        │
│        │ │        │ │        │  │ FROM nginx:stable-  │
│ Vite   │ │vitest  │ │tsc -b  │  │ alpine              │
│ --host │ │ run    │ │vite    │  │ COPY dist/ from     │
│(HMR)   │ │(exits) │ │ build  │  │ build stage         │
└────────┘ └────────┘ └────┬───┘  │ COPY nginx.conf     │
                           │      └────────────────────┘
                           └──── COPY --from=build /app/dist
```

**Why `node:22-alpine`?** Alpine Linux images are ~5 MB vs ~200 MB for Debian. Node.js is only needed to compile — not in the final image. The `runtime` stage uses `nginx:stable-alpine` (~25 MB), which does not contain Node.js at all.

### 4.4 Image size comparison

| Stage | Base image | Final size |
|---|---|---|
| `build` (backend) | dotnet/sdk:10.0 | ~900 MB — never shipped |
| `runtime` (backend) | dotnet/aspnet:10.0 | ~250 MB — what runs in production |
| `deps` (frontend) | node:22-alpine | ~200 MB — never shipped |
| `runtime` (frontend) | nginx:stable-alpine | ~50 MB — what runs in production |

---

## 5. Docker Compose: Orchestrating Services

### 5.1 What Docker Compose does

Docker Compose reads a `docker-compose.yml` file and manages a group of containers as a single application unit. Key operations:

```bash
docker compose up --build   # build images + start all containers
docker compose down         # stop and remove containers
docker compose logs -f      # stream logs from all services
docker compose ps           # list running containers and their ports
docker compose restart backend  # restart a single service
```

### 5.2 The compose file structure

```yaml
services:       # containers to run
  db:           # service name (also the DNS hostname inside the network)
    image: ...  # pre-built image from a registry
    build: ...  # OR: build from a Dockerfile
    environment: # env vars injected into the container
    ports:       # "host:container" port mapping
    volumes:     # mounts: named volumes or bind mounts
    depends_on:  # startup ordering + health conditions
    networks:    # which networks this service joins
    healthcheck: # command to verify the container is ready

volumes:        # named volumes (persistent, Docker-managed)

networks:       # virtual networks that connect services
```

### 5.3 Override files

Docker Compose supports layering multiple files:

```bash
# Development (default):
docker compose up

# Production (base + overrides):
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

The second file **overrides** matching keys in the first. Only changed values need to be listed. RetailStore uses this to switch from the Vite dev server to an Nginx production build without duplicating the entire compose definition.

### 5.4 Profiles (not used here, but worth knowing)

Docker Compose supports `profiles` to group services. Services in a profile only start when that profile is explicitly activated:

```bash
docker compose --profile tools up   # start only "tools" profile services
```

RetailStore uses a separate `docker-compose.test.yml` file instead of profiles for test runners — this keeps the test config completely isolated from the development and production configs.

---

## 6. Networking

### 6.1 Bridge networks

By default, every `docker-compose.yml` creates a **bridge network** that connects all its services. Services communicate over this network using their **service name as a DNS hostname**.

```yaml
networks:
  retailstore_net:
    driver: bridge
```

Inside the `retailstore_net` network:
- The backend container resolves `db` → SQL Server container's IP
- The frontend container resolves `backend` → .NET API container's IP
- No ports need to be exposed on the host for inter-service communication

This is the Docker equivalent of Kubernetes DNS (`service-name.namespace.svc.cluster.local`).

### 6.2 Port mapping

Port mapping (`host:container`) makes a container port reachable from the machine running Docker:

```yaml
ports:
  - "5240:8080"   # localhost:5240 on the host → port 8080 inside the container
  - "3000:3000"   # localhost:3000 → port 3000 in the container
  - "1433:1433"   # SQL Server accessible from SSMS / Azure Data Studio
```

Container-to-container traffic uses the internal container port directly — no host port mapping needed. The frontend container reaches the backend at `http://backend:8080`, not at `localhost:5240`.

### 6.3 The Vite proxy in Docker

In development, the browser runs on the host machine and makes API calls to `http://localhost:3000/api/*`. Vite's dev server intercepts these calls and proxies them to the backend.

The proxy target must be different depending on where Vite is running:

| Environment | Vite runs on | Backend address |
|---|---|---|
| Local (no Docker) | Host machine | `http://localhost:5240` |
| Docker Compose | Container in `retailstore_net` | `http://backend:8080` |

**`/retail-store-frontend/vite.config.ts`** handles this with a single environment variable:

```typescript
server: {
  host: true,   // bind to 0.0.0.0 — required inside Docker (see §10.3)
  port: 3000,
  proxy: {
    '/api': {
      target: process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5240',
      changeOrigin: true,
    },
  },
},
```

In `docker-compose.yml`:
```yaml
frontend:
  environment:
    VITE_API_PROXY_TARGET: http://backend:8080
```

`VITE_API_PROXY_TARGET` is a **Node.js process environment variable** — read at Vite startup, not compiled into the bundle. This is intentional: the Vite proxy is a server-side concern, not a browser bundle concern.

### 6.4 Network isolation between services

The `db` container has its port `1433` mapped to the host (`1433:1433`) so developers can connect from SSMS or Azure Data Studio. However, from the backend container's perspective, SQL Server is only reachable via `db:1433` — the internal network address. This provides an extra layer of isolation: even if the host port mapping were removed, inter-service communication would still work.

---

## 7. Volumes & Data Persistence

### 7.1 Named volumes vs bind mounts

| | Named volume | Bind mount |
|---|---|---|
| Managed by | Docker | You |
| Location | Docker's internal storage area | An exact path on your machine |
| Data survives `docker compose down` | Yes | Yes |
| Data survives `docker compose down -v` | No | Yes |
| Good for | Database files, persistent state | Hot-reload source code, config files |
| Cross-platform | Yes | Path syntax differs on Windows vs Linux |

RetailStore uses a **named volume** for SQL Server data:

```yaml
volumes:
  sqlserver_data:/var/opt/mssql   # Docker manages where the files live on the host
```

### 7.2 The SQL Server volume lifecycle

```
docker compose up           → container starts, volume attached
docker compose stop         → container stopped, volume KEPT
docker compose down         → container removed, volume KEPT ✓
docker compose down -v      → container removed, volume DELETED ✗
docker compose up           → container recreates, volume reattached
```

**The data always survives normal operations.** You must explicitly pass `-v` to destroy it. This is used deliberately when you need a completely clean database (e.g., after seeder changes).

### 7.3 Why SQL Server needs a volume but the API and frontend do not

- **SQL Server** stores its `.mdf` / `.ldf` data files on disk. Without a volume, every `docker compose down` would wipe the entire database.
- **The .NET API** is stateless — all state is in SQL Server. No volume needed.
- **The Nginx frontend** only serves static files baked into the image. No volume needed.
- **The Vite dev server** serves files from its container filesystem. For hot reload without volumes, rebuild the image; for live editing with volumes, a bind mount can be added (`./retail-store-frontend/src:/app/src`).

### 7.4 Inspecting and managing volumes

```bash
docker volume ls                         # list all Docker volumes
docker volume inspect retailstore_sqlserver_data  # full details (mount point, driver)
docker volume rm retailstore_sqlserver_data       # delete manually (if not in use)
```

---

## 8. Configuration & Secrets

### 8.1 The `.env` file

Docker Compose automatically reads a `.env` file in the same directory. All variables declared there become available for `${VAR}` substitution anywhere in `docker-compose.yml`.

**`/.env`** (never committed to git):
```dotenv
DB_PASSWORD=V!8rK#2mQ@7xLp$4Nz
JWT_SECRET=dK8YvM7WQh9rL2fZgP3xNc6TUa0+JeR5sBy1Xk4mHvGq8FwDnCt9Zi2AoEl7YuSb
SEED_ADMIN_EMAIL=admin@retailstore.com
```

**`/.env.example`** (committed to git — the template):
```dotenv
DB_PASSWORD=YourStr0ngPassword!
JWT_SECRET=REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS
SEED_ADMIN_EMAIL=admin@retailstore.com
```

Onboarding a new developer: `Copy-Item .env.example .env` then fill in real values.

### 8.2 Variable substitution in docker-compose.yml

```yaml
db:
  environment:
    SA_PASSWORD: "${DB_PASSWORD}"      # reads from .env → V!8rK#2mQ@7xLp$4Nz
```

Docker Compose substitutes `${DB_PASSWORD}` before passing the value to the container. The container itself sees only the resolved value — it never knows about `.env`.

### 8.3 ASP.NET Core configuration hierarchy

ASP.NET Core reads configuration from multiple sources in order, with later sources overriding earlier ones:

```
1. appsettings.json              ← lowest priority (committed to git)
2. appsettings.{Environment}.json
3. User Secrets (development only)
4. Environment variables          ← highest priority (injected by Docker)
5. Command-line arguments
```

Environment variables use `__` (double underscore) as the hierarchy separator. So:

```yaml
# In docker-compose.yml:
environment:
  ConnectionStrings__DefaultConnection: "Server=db,1433;..."
  Jwt__Secret: "${JWT_SECRET}"
  Jwt__Issuer: RetailStore
```

These map to:
```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=db,1433;..." },
  "Jwt": { "Secret": "...", "Issuer": "RetailStore" }
}
```

No changes to `appsettings.json` are needed. The Docker environment variable silently overrides the `localdb` connection string at runtime.

### 8.4 The connection string transformation

| Environment | Who sets it | Value |
|---|---|---|
| Local development | `appsettings.json` | `Server=(localdb)\MSSQLLocalDB;...;Trusted_Connection=True` |
| Docker (all environments) | `docker-compose.yml` environment block | `Server=db,1433;...;User Id=sa;Password=...` |

Two key differences when moving to Docker:
1. **Server name**: `(localdb)\MSSQLLocalDB` → `db,1433` (`db` = Docker service name, `1433` = SQL Server port)
2. **Authentication**: Windows Authentication (`Trusted_Connection=True`) → SQL Authentication (`User Id=sa;Password=...`) — Linux containers have no Windows identity concept

### 8.5 `$$` escaping in docker-compose.yml

Docker Compose performs variable substitution on every string in the file. To use a literal `$` that the container's shell should expand (not Compose), double it:

```yaml
# healthcheck test command in docker-compose.yml
test:
  - CMD-SHELL
  - '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$SA_PASSWORD" -Q "SELECT 1" -b -No -C'
```

Processing:
1. Docker Compose sees `$$SA_PASSWORD` → converts to `$SA_PASSWORD` (literal dollar sign)
2. The shell inside the container sees `-P "$SA_PASSWORD"` → expands to the actual password

This is why `$$` is used in the healthcheck command rather than `${DB_PASSWORD}`. Both work; `$$SA_PASSWORD` reads from the container's own environment (set by the `SA_PASSWORD: "${DB_PASSWORD}"` line), which is slightly more correct — the healthcheck uses the same password that SQL Server was configured with.

---

## 9. Health Checks & Service Dependencies

### 9.1 Why SQL Server needs a health check

SQL Server on Linux takes **30–45 seconds** to initialize on first boot. It must:
1. Start the SQL Server engine
2. Recover any existing databases
3. Begin accepting TCP connections on port 1433

If the backend container starts during this window, `EnsureCreatedAsync()` (called by `DatabaseSeeder.SeedAsync`) throws a connection error. The container crashes and restarts — wasting time and filling logs with noise.

### 9.2 The health check implementation

```yaml
db:
  healthcheck:
    test:
      - CMD-SHELL
      - '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$SA_PASSWORD" -Q "SELECT 1" -b -No -C'
    interval: 10s       # run every 10 seconds
    timeout: 5s         # fail if it takes longer than 5s
    retries: 10         # mark unhealthy after 10 consecutive failures
    start_period: 45s   # do not count failures during the first 45s
```

`sqlcmd` is the SQL Server CLI bundled in the image. It connects and runs `SELECT 1`. If the query succeeds, the exit code is 0 (healthy). If SQL Server is not ready, `sqlcmd` exits non-zero (unhealthy).

`start_period: 45s` is critical: without it, the first 4–5 inevitable failures during SQL Server's initialization would quickly exhaust the `retries: 10` budget, marking the container permanently unhealthy before it has had a chance to start.

### 9.3 `depends_on` conditions

```yaml
backend:
  depends_on:
    db:
      condition: service_healthy   # wait for the healthcheck to pass
```

| Condition | Meaning |
|---|---|
| `service_started` | Container is running (default — not enough for slow services) |
| `service_healthy` | Container passed its healthcheck — the service is genuinely ready |
| `service_completed_successfully` | Container exited with code 0 — for one-shot jobs |

Without `service_healthy`, `depends_on: db` would just mean "start `db` before `backend`" — Docker would launch the backend almost immediately after starting the SQL Server container, before SQL Server is actually listening.

### 9.4 Container startup sequence for RetailStore

```
1. docker compose up

2. db starts → SQL Server initializing...
   [healthcheck] SELECT 1 → fail (×4-5)
   [45s start_period] failures ignored
   [healthcheck] SELECT 1 → success ✓  db is HEALTHY

3. backend starts (depends_on: db: condition: service_healthy)
   └─ DatabaseSeeder.SeedAsync() → EnsureCreatedAsync() ✓
   └─ DevelopmentDataSeeder.SeedAsync() ✓
   └─ Kestrel listening on 0.0.0.0:8080

4. frontend starts (depends_on: backend)
   └─ Vite dev server starts on 0.0.0.0:3000
   └─ proxy: /api/* → http://backend:8080
```

---

## 10. Development Workflow

### 10.1 Architecture in development mode

```
┌──────────── Host Machine ────────────────────────────────────────┐
│  Browser                                                          │
│  http://localhost:3000/orders  → ─────────────────────────────┐  │
│  http://localhost:3000/api/products → ─────────────────────┐  │  │
│                                                             │  │  │
│  ┌─────────────────────── Docker: retailstore_net ─────────┼──┼──┤
│  │                                                         │  │  │
│  │  ┌──────────────────────────────────┐                   │  │  │
│  │  │  frontend (Vite dev server)      │◄──────────────────┘  │  │
│  │  │  node:22-alpine                  │                      │  │
│  │  │  port 3000                       │  HMR websocket       │  │
│  │  │  proxy /api/* → backend:8080     │──────────────────────┘  │
│  │  └────────────────┬─────────────────┘  /api/* proxied         │
│  │                   │                                            │
│  │  ┌────────────────▼─────────────────┐                         │
│  │  │  backend (.NET 10 API)           │                         │
│  │  │  dotnet/aspnet:10.0             │                         │
│  │  │  port 8080 (exposed as 5240)    │                         │
│  │  └────────────────┬─────────────────┘                         │
│  │                   │ SQL Server connection                      │
│  │  ┌────────────────▼─────────────────┐                         │
│  │  │  db (SQL Server 2022)            │                         │
│  │  │  mssql/server:2022-latest       │                         │
│  │  │  port 1433                       │                         │
│  │  │  volume: sqlserver_data          │                         │
│  │  └──────────────────────────────────┘                         │
│  └────────────────────────────────────────────────────────────────┤
└──────────────────────────────────────────────────────────────────┘
```

### 10.2 The Vite dev server inside Docker

The Vite dev server runs inside a container using the `dev` stage of the frontend Dockerfile:

```dockerfile
FROM deps AS dev
COPY . .
EXPOSE 3000
CMD ["npm", "run", "dev", "--", "--host"]
```

`--host` passes the `--host` flag to the Vite CLI, which binds Vite to `0.0.0.0` (all network interfaces). Without this, Vite only listens on `127.0.0.1` — the container's own loopback. Docker's port mapping (`3000:3000`) works by routing traffic from the host to the container's network interface, but the container's loopback is not accessible from outside the container.

`vite.config.ts` also sets `host: true` to achieve the same effect without requiring the CLI flag — both are present for redundancy.

### 10.3 Hot-Module Replacement (HMR) inside Docker

HMR works over a WebSocket connection from the browser to the Vite dev server. When running Vite inside Docker, the HMR WebSocket connects to `localhost:3000` (the host-side port mapping), which Docker routes to the container. This usually works automatically.

If HMR is not working (edits don't appear), the issue is typically a polling interval setting. Add this to `vite.config.ts`:

```typescript
server: {
  watch: {
    usePolling: true,   // needed when source files are on a Windows NTFS volume
  }
}
```

Windows NTFS does not propagate `inotify` filesystem events to Linux containers. Polling is slower (~100–300ms delay vs instant) but reliable. For best performance, place the project on WSL2's native ext4 filesystem (`\\wsl$\Ubuntu\home\...`).

### 10.4 Rebuilding after code changes

The `dev` stage copies source code into the image at build time. To pick up code changes you must rebuild:

```powershell
docker compose up --build frontend    # rebuild only the frontend
docker compose up --build backend     # rebuild only the backend
docker compose up --build             # rebuild everything
```

For a faster inner loop while developing:
- Run the **frontend natively** (`npm run dev` on your machine) — instant HMR, no rebuild
- Run only **SQL Server + backend in Docker** — no need to manage a local DB
- Use `docker compose up db backend` to start only those two services

---

## 11. Production Workflow

### 11.1 Architecture in production mode

```
┌──────────── Host Machine / Server ───────────────────────────────┐
│  Browser                                                          │
│  http://server:3000/orders → ─────────────────────────────────┐  │
│  http://server:3000/api/products → ───────────────────────────┼──┤
│                                                                │  │
│  ┌─────────────────────── Docker: retailstore_net ────────────┼──┤
│  │                                                            │  │
│  │  ┌──────────────────────────────────────────────────┐     │  │
│  │  │  frontend (Nginx)                                │◄────┘  │
│  │  │  nginx:stable-alpine                             │        │
│  │  │  port 80 (exposed as 3000)                      │        │
│  │  │  serves /usr/share/nginx/html (compiled dist/)  │        │
│  │  │  location /api/ → proxy retailstore-backend-svc │        │
│  │  │  location / → try_files $uri /index.html         │        │
│  │  └────────────────────┬────────────────────────────┘        │
│  │                        │ /api/* proxied                      │
│  │  ┌─────────────────────▼──────────────────────────┐         │
│  │  │  backend (.NET 10 API)                          │         │
│  │  │  port 8080                                       │         │
│  │  └─────────────────────┬──────────────────────────┘         │
│  │                        │                                     │
│  │  ┌─────────────────────▼──────────────────────────┐         │
│  │  │  db (SQL Server 2022) + sqlserver_data volume   │         │
│  │  └────────────────────────────────────────────────┘         │
│  └────────────────────────────────────────────────────────────── │
└──────────────────────────────────────────────────────────────────┘
```

### 11.2 The production frontend: why Nginx

In production the Vite dev server is gone. The `npm run build` command compiles TypeScript and bundles all JavaScript, CSS, and assets into the `dist/` directory. These are static files — no Node.js runtime is needed to serve them.

Nginx is a high-performance web server purpose-built for static file serving. It handles thousands of concurrent requests with negligible CPU usage, compared to a Node.js server which would need to process each request through JavaScript.

The `runtime` stage copies `dist/` into the Nginx image:

```dockerfile
FROM nginx:stable-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
```

### 11.3 The Nginx configuration

**`/retail-store-frontend/nginx.conf`** serves two purposes:

**SPA fallback routing:**
```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

React Router handles navigation client-side. If a user bookmarks `/orders/123` and opens it directly, the browser sends a real HTTP GET to Nginx. No file named `/orders/123` exists on disk — without this rule, Nginx returns 404. `try_files` falls back to `index.html`, React Router reads the URL, and renders the correct page.

**API reverse proxy:**
```nginx
location /api/ {
    proxy_pass http://retailstore-backend-svc/api/;
}
```

> **Note:** `nginx.conf` currently uses `retailstore-backend-svc` — the Kubernetes Service name set during the K8s migration (documented in `KUBERNETES.md`). For a Docker-only production deployment, this should be `http://backend:8080/api/`. When deploying with Docker Compose, update this line accordingly. When deploying with Kubernetes (via the Helm chart), the current value is correct.

**Static asset caching:**
```nginx
location ~* \.(js|css|png|jpg|svg|ico|woff|woff2|ttf|eot)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

Vite adds a content hash to every asset filename (e.g., `main.a3f2c1.js`). The hash changes when the file content changes, guaranteeing browsers always fetch the latest version. It is therefore safe to tell browsers to cache assets for one year — cutting bandwidth and improving load times for returning users.

### 11.4 HTTPS inside containers

The backend uses `ASPNETCORE_URLS=http://+:8080` to listen on HTTP only. `app.UseHttpsRedirection()` in `Program.cs` would redirect to HTTPS that the container does not serve.

This is intentional. TLS should be **terminated at the edge** — by a reverse proxy (Nginx on the host, a cloud load balancer, or a Kubernetes Ingress). Traffic inside the Docker network is on a private bridge — it never leaves the host machine unencrypted.

In a production deployment, the typical setup is:
```
Internet → Nginx (port 443, TLS) → Docker container (port 80, HTTP)
```

---

## 12. Testing with Docker

### 12.1 The test strategy

RetailStore has three test layers, each handled differently in Docker:

| Layer | Tool | DB needed | Docker approach |
|---|---|---|---|
| Backend unit tests | xUnit + Moq | No | `dotnet test` in `Dockerfile` test stage |
| Backend integration tests | xUnit + WebApplicationFactory | SQLite in-memory | `dotnet test` in `Dockerfile` test stage (same stage — SQLite is embedded) |
| Frontend unit tests | Vitest | No | `npm run test` in frontend `Dockerfile` test stage |
| E2E tests | Playwright | Real backend + DB | Run locally (`npm run e2e`); not in Docker CI |

### 12.2 The key insight: SQLite in-memory

Integration tests would normally require a real SQL Server instance — making them expensive to run in Docker (you'd need a full `db` service, health checks, and connection strings).

`RetailStoreWebAppFactory` (in `tests/RetailStore.Tests/Integration/RetailStoreWebAppFactory.cs`) replaces this:

```csharp
// Removes the SQL Server EF Core provider registered in Program.cs
foreach (var d in services.Where(d => d.ServiceType == optionsConfigType).ToList())
    services.Remove(d);

// Registers SQLite shared in-memory instead
services.AddDbContext<RetailStoreDbContext>((sp, opts) =>
    opts.UseSqlite(ConnectionString));  // "DataSource=Test_abc;Mode=Memory;Cache=Shared"
```

Each test class gets its own uniquely-named in-memory database. Tests run in isolation without any external service. This means:

- The backend `Dockerfile` `test` stage needs no SQL Server container
- `docker-compose.test.yml` has no `db-test` service
- Tests run in ~30 seconds instead of waiting 45 seconds for SQL Server to start

### 12.3 `docker-compose.test.yml`

```yaml
services:
  backend-tests:
    build:
      context: .
      dockerfile: Dockerfile
      target: test    # runs: dotnet test ... --logger "console;verbosity=normal"

  frontend-tests:
    build:
      context: ./retail-store-frontend
      dockerfile: Dockerfile
      target: test    # runs: npm run test (vitest run)
```

No networks, no volumes, no health checks. Each service builds its test stage, runs tests, and exits with the test result's exit code.

### 12.4 The `--exit-code-from` flag

```bash
docker compose -f docker-compose.test.yml up --build --exit-code-from backend-tests
```

Normally `docker compose up` exits with code 0 regardless of what happened inside containers. `--exit-code-from backend-tests` changes this: Compose exits with the same code as the `backend-tests` container. Exit code 0 = all tests passed. Exit code 1 = at least one test failed.

CI systems (GitHub Actions, Azure Pipelines, Jenkins) use the exit code of each step to mark it as passed or failed. This flag is what connects Docker test results to CI pipeline outcomes.

---

## 13. What Was Actually Built

### Files Created

#### Root level

| File | What it does |
|---|---|
| `Dockerfile` | Backend 4-stage image: `build` → `test` → `publish` → `runtime` |
| `.dockerignore` | Excludes `retail-store-frontend/`, `**/bin`, `**/obj`, `.env` from the backend build context |
| `.env.example` | Committed template; developers copy this to `.env` and fill in real secrets |
| `docker-compose.yml` | Development stack: SQL Server + .NET backend + Vite dev server |
| `docker-compose.prod.yml` | Production overrides: Nginx frontend, `ASPNETCORE_ENVIRONMENT=Production`, `restart: unless-stopped` |
| `docker-compose.test.yml` | Standalone test runner: backend-tests + frontend-tests (no SQL Server needed) |

#### `retail-store-frontend/`

| File | What it does |
|---|---|
| `Dockerfile` | Frontend 5-stage image: `deps` → `dev` → `test` → `build` → `runtime` |
| `.dockerignore` | Excludes `node_modules/`, `dist/`, Playwright artifacts from the frontend build context |
| `nginx.conf` | Nginx config for SPA fallback routing, API proxy, and static asset caching |

#### `src/RetailStore.Api/Configuration/`

| File | What it does |
|---|---|
| `DevelopmentDataSeeder.cs` | Seeds demo data (users, products, inventory, customers, providers, orders) when `ASPNETCORE_ENVIRONMENT=Development`. Idempotent — skips if products already exist. |

### Files Modified

| File | Change |
|---|---|
| `.gitignore` | Added `.env` and `.env.local` to prevent secrets from entering git |
| `retail-store-frontend/vite.config.ts` | Added `host: true` (binds to `0.0.0.0` for Docker); proxy target reads `process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5240'` |
| `src/RetailStore.Api/Program.cs` | Added `DevelopmentDataSeeder.SeedAsync()` call after `DatabaseSeeder.SeedAsync()`, guarded by `app.Environment.IsDevelopment()` |
| `retail-store-frontend/nginx.conf` | Proxy target updated from `http://backend:8080/api/` to `http://retailstore-backend-svc/api/` for Kubernetes compatibility (see `KUBERNETES.md §12`) |

### Seeded development data (first `docker compose up --build`)

| Entity | Records |
|---|---|
| Users | `admin@retailstore.com` (Admin role), `john.doe@retailstore.com` (Staff), `jane.smith@retailstore.com` (Staff) |
| Providers | TechSupply Co, Global Goods Ltd, Fresh Products Inc |
| Products | Laptop Pro 15", Wireless Mouse, Standing Desk, Office Chair, USB-C Hub 6-Port, Mechanical Keyboard |
| Inventory | 1 stock entry per product (8–50 units, with reorder thresholds) |
| Customers | Alice Johnson, Bob Martinez, Carol Williams, David Brown |
| Orders | 4 orders with items (Draft status) |

---

## 14. How the App Benefits from This

### Before Docker

| Problem | Impact |
|---|---|
| LocalDB only works on Windows | macOS/Linux developers cannot run the project at all |
| "Install SQL Server" in a README | 30+ minute onboarding, version mismatches between developers |
| Hardcoded connection string in `appsettings.json` | Every developer edits the file, creates git conflicts |
| JWT secret committed to git | Anyone with repo access can forge tokens |
| Manual `Seed:AdminEmail` config in `appsettings.Development.json` | First login requires reading setup docs carefully |
| Tests may use a shared local DB | Test isolation breaks; one test's data affects another's |
| No way to reproduce CI failures locally | "It passes locally, fails in CI" — unreproducible environment |

### After Docker

| Benefit | How it works in this project |
|---|---|
| **One-command setup** | `docker compose up --build` — SQL Server, backend, frontend, all seeded. No manual config. |
| **Cross-platform** | SQL Server Linux container runs identically on Windows, macOS (Apple Silicon via Rosetta), Linux |
| **Secret isolation** | Passwords and JWT keys live in `.env` (gitignored). `appsettings.json` still has the `localdb` default for anyone running without Docker. |
| **Reproducible test environment** | `docker-compose.test.yml` runs the exact same code in the exact same environment as CI — no "works locally, fails in CI" |
| **Data persistence** | Named volume `sqlserver_data` survives `docker compose down`. Data is destroyed only when you choose to with `down -v`. |
| **Environment parity** | `docker-compose.prod.yml` uses the same images as `docker-compose.yml` — the only difference is the environment variables and the frontend build target. |
| **Ready-to-use demo data** | Admin credentials and demo records seeded automatically on first run. No SQL scripts to run manually. |
| **Frontend hot reload in Docker** | `VITE_API_PROXY_TARGET` switches the backend URL automatically. Same `npm run dev` command works inside and outside Docker. |
| **CI test speed** | Backend integration tests use SQLite in-memory — no SQL Server setup, no 45-second wait. Tests finish in ~30 seconds. |

---

## 15. Running in Development (Local)

### Prerequisites

- Docker Desktop installed and running (green whale icon in system tray)
- Kubernetes does **not** need to be enabled in Docker Desktop for this setup

### First run

```powershell
# 1. Clone the repository and navigate to the project root
cd c:\Users\fernando.vela\Documents\RetailStore

# 2. Create your local secrets file (one time only)
Copy-Item .env.example .env
# Edit .env — set a strong DB_PASSWORD and a unique JWT_SECRET

# 3. Build images and start all services
docker compose up --build
```

Expected output sequence:
```
[+] Building ...
  retailstore-db     Pulling ... Done
  retailstore-backend Building ...
  retailstore-frontend Building ...
[+] Running 3/3
  ✔ Container retailstore-db        Healthy
  ✔ Container retailstore-backend   Started
  ✔ Container retailstore-frontend  Started
```

Wait for the backend log line:
```
Now listening on: http://[::]:8080
```

Then open:
- **React app**: `http://localhost:3000`
- **Swagger UI**: `http://localhost:5240/swagger`
- **Health check**: `http://localhost:5240/health`

### Login credentials (seeded on first run)

| Email | Password | Role |
|---|---|---|
| `admin@retailstore.com` | `Admin@RetailStore1!` | Admin (full access) |
| `john.doe@retailstore.com` | `Staff@RetailStore1!` | Staff |
| `jane.smith@retailstore.com` | `Staff@RetailStore1!` | Staff |

### Connecting to the Docker database from a GUI tool

Open SSMS, Azure Data Studio, or any SQL client:
- **Server**: `localhost,1433`
- **Authentication**: SQL Server Authentication
- **Login**: `sa`
- **Password**: the `DB_PASSWORD` from your `.env` file

This is the Docker database — completely separate from any local `(localdb)\MSSQLLocalDB` instance.

### Starting fresh (wipe all data)

```powershell
docker compose down -v        # removes containers AND the sqlserver_data volume
docker compose up --build     # rebuilds and re-seeds from scratch
```

---

## 16. Deploying to Production

### 16.1 Single-server production deployment

For a single server (a VPS, an EC2 instance, an Azure VM), Docker Compose is sufficient.

```powershell
# On the production server:

# 1. Copy the project files (or git pull)
git clone <repo-url>
cd RetailStore

# 2. Set production secrets in .env
nano .env
# DB_PASSWORD=<strong production password>
# JWT_SECRET=<strong random secret>
# SEED_ADMIN_EMAIL=admin@retailstore.com

# 3. Build production images and start in detached mode
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
```

The `docker-compose.prod.yml` applies these changes on top of the base file:

| Setting | Dev value | Prod value |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `frontend` build target | `dev` (Vite dev server) | `runtime` (Nginx) |
| `frontend` port mapping | `3000:3000` | `3000:80` |
| All services `restart` | (not set) | `unless-stopped` |

`restart: unless-stopped` means Docker automatically restarts containers if they crash or if the server reboots. Combined with Docker Desktop's "Start on login" option, the entire RetailStore stack survives server restarts without any intervention.

### 16.2 Differences from development

| Aspect | Development | Production |
|---|---|---|
| Frontend | Vite dev server (HMR, sourcemaps) | Nginx serving compiled `dist/` |
| Backend | `ASPNETCORE_ENVIRONMENT=Development` (Swagger enabled) | `ASPNETCORE_ENVIRONMENT=Production` (Swagger disabled) |
| Logging | Verbose | Information level only |
| Dev data seeder | Runs on startup | Does not run (env check: `IsDevelopment()`) |
| Swagger UI | `http://localhost:5240/swagger` | Disabled in Production env |

### 16.3 Updating the application

When you push new code and want to update the running production containers:

```powershell
# On the production server:

git pull origin master

# Rebuild and restart only the changed service(s)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d backend
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d frontend

# Verify the new containers are running
docker compose ps
```

> **Note:** `docker compose up --build -d` on a running stack will stop the old container and start the new one. There is a brief downtime (~2–5 seconds) during the container replacement. For zero-downtime deployments, use Kubernetes rolling updates (see `KUBERNETES.md`).

---

## 17. Day-to-Day Operations Reference

### Checking status

```powershell
# All running containers + their ports
docker compose ps

# Health status of all services
docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

# Real-time logs from all services (Ctrl+C to stop)
docker compose logs -f

# Logs from a single service
docker compose logs -f backend
docker compose logs -f db
docker compose logs -f frontend

# Last 50 lines from the backend
docker compose logs --tail=50 backend
```

### Inspecting containers

```powershell
# Open a shell inside the running backend container
docker compose exec backend bash

# Open a shell in the SQL Server container (for sqlcmd)
docker compose exec db bash
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'your_password' -No -C

# Check environment variables in a running container
docker compose exec backend env | Sort-Object

# Check resource usage (CPU + memory per container)
docker stats
```

### Rebuilding individual services

```powershell
# Rebuild backend only (after .NET code changes when not using hot reload)
docker compose up --build backend

# Rebuild frontend only (after adding new npm packages)
docker compose up --build frontend

# Force a full rebuild ignoring cache (when a base image was updated)
docker compose build --no-cache
docker compose up
```

### Database operations

```powershell
# Connect from SSMS: localhost,1433 / sa / <DB_PASSWORD>

# Or use sqlcmd from inside the container:
docker compose exec db /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "$env:DB_PASSWORD" -No -C `
  -Q "SELECT name FROM sys.databases"

# Reset the database completely (DESTRUCTIVE — all data deleted)
docker compose down -v
docker compose up --build
```

### Running tests in Docker

```powershell
# Run all tests (backend + frontend), exit with test result code
docker compose -f docker-compose.test.yml up --build --exit-code-from backend-tests

# Run only backend tests
docker compose -f docker-compose.test.yml run --rm backend-tests

# Run only frontend tests
docker compose -f docker-compose.test.yml run --rm frontend-tests

# Clean up test containers
docker compose -f docker-compose.test.yml down
```

### Cleaning up Docker resources

```powershell
# Remove stopped containers, unused networks, dangling images
docker system prune

# Remove all unused images (not just dangling) — frees significant disk space
docker system prune -a

# Remove only the RetailStore stack (containers + network, keep volume)
docker compose down

# Remove stack + volume (all data deleted)
docker compose down -v

# List all volumes
docker volume ls

# List all images
docker images
```

### Key lessons learned during implementation

| Issue | Root cause | Fix |
|---|---|---|
| SQL Server container marked unhealthy immediately | `retries: 10` exhausted by the expected failures during the 45s initialization window | Added `start_period: 45s` to the healthcheck — Docker ignores failures during this period |
| Backend crashes on startup with connection refused | `depends_on: db` used `service_started` (default) instead of `service_healthy` | Changed to `depends_on: db: condition: service_healthy` |
| Vite dev server not reachable inside Docker | Vite bound to `127.0.0.1` (loopback) by default | Added `host: true` to `vite.config.ts` and `--host` flag to `CMD` in Dockerfile |
| Vite proxy still targeting `localhost:5240` inside Docker | `localhost` inside a container is the container itself, not the backend container | Added `VITE_API_PROXY_TARGET=http://backend:8080` env var to the frontend service in `docker-compose.yml` |
| `$$SA_PASSWORD` in healthcheck empty | Docker Compose substituted `$SA_PASSWORD` as a Compose variable (undefined → empty) before the shell saw it | Used `$$SA_PASSWORD` — Compose converts `$$` to `$`, then the container shell expands it correctly |
| Admin user not getting Admin role on first run | `DatabaseSeeder.SeedAsync` ran before `DevelopmentDataSeeder.SeedAsync` — admin user didn't exist yet | `DevelopmentDataSeeder` creates users and calls `user.AssignRole()` in the same change-tracker batch, then `DatabaseSeeder` is idempotent on the second check |
| `docker compose down -v` accidentally deleted data | Developer ran the command without realizing `-v` removes volumes | Documented clearly: `down` = safe, `down -v` = destructive |

---

## 18. Docker Compose → Kubernetes Migration Path

When the application outgrows a single server and needs high availability, auto-scaling, or multi-node distribution, the Docker Compose setup maps cleanly to Kubernetes. See `KUBERNETES.md` for the full migration.

| Docker Compose concept | Kubernetes equivalent | In RetailStore |
|---|---|---|
| `services.db` | StatefulSet + PVC + Service | `retailstore-sqlserver` StatefulSet |
| `services.backend` | Deployment + Service | `retailstore-backend` Deployment |
| `services.frontend` | Deployment + Service | `retailstore-frontend` Deployment |
| `ports: "5240:8080"` | Ingress + ClusterIP Service | NGINX Ingress routes `/api/*` |
| `ports: "3000:80"` | Ingress + ClusterIP Service | NGINX Ingress routes `/*` |
| `volumes: sqlserver_data` | PersistentVolumeClaim | `sqlserver-data` PVC, 10 Gi |
| `.env` file | ConfigMap + Secret | `retailstore-config` + `retailstore-secrets` |
| `environment:` block | `envFrom` + `env` in container spec | `envFrom: configMapRef / secretRef` |
| `depends_on: condition: service_healthy` | InitContainer | `wait-for-sqlserver` init polls port 1433 |
| `networks: retailstore_net` | Cluster flat network | Service DNS: `svc-name.namespace.svc.cluster.local` |
| `restart: unless-stopped` | `restartPolicy: Always` | Default for Deployment pods |
| `docker-compose.test.yml` | GitHub Actions `dotnet test` job | `.github/workflows/ci-cd.yml` |
| `docker-compose.yml` (whole file) | Helm chart | `k8s/helm/retailstore/` |
| `docker-compose.prod.yml` | `values-prod.yaml` | `k8s/helm/retailstore/values-prod.yaml` |
