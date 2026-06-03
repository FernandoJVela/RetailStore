# Kubernetes: Theory & Practical Guide for RetailStore

This document covers all Kubernetes theory a software developer/architect must know, then applies every concept directly to the RetailStore project (ASP.NET Core 10 backend + React/Nginx frontend + SQL Server).

---

## Table of Contents

1. [Why Kubernetes? (And When Not to Use It)](#1-why-kubernetes-and-when-not-to-use-it)
2. [Core Architecture](#2-core-architecture)
3. [Workload Resources](#3-workload-resources)
4. [Networking](#4-networking)
5. [Configuration & Secrets](#5-configuration--secrets)
6. [Storage](#6-storage)
7. [Health, Scheduling & Resource Management](#7-health-scheduling--resource-management)
8. [Observability](#8-observability)
9. [Security](#9-security)
10. [Helm: The Package Manager](#10-helm-the-package-manager)
11. [CI/CD with Kubernetes](#11-cicd-with-kubernetes)
12. [What Was Actually Built](#12-what-was-actually-built)
13. [How the App Benefits from This](#13-how-the-app-benefits-from-this)
14. [Running in Development (Local)](#14-running-in-development-local)
15. [Deploying to Production via GitHub](#15-deploying-to-production-via-github)
16. [Day-to-Day Operations Reference](#16-day-to-day-operations-reference)
17. [Docker Compose → Kubernetes Mapping](#17-docker-compose--kubernetes-mapping)

---

## 1. Why Kubernetes? (And When Not to Use It)

### What problem does Kubernetes solve?

Docker Compose runs containers on a **single machine**. When you need:

- **High availability** — keep the app running even when a machine or container crashes
- **Horizontal scaling** — run 10 copies of the backend during peak load, 1 at night
- **Zero-downtime deploys** — roll out new images without dropping requests
- **Self-healing** — restart crashed containers automatically
- **Multi-node clusters** — spread workloads across many machines

...you need an **orchestrator**. Kubernetes (K8s) is the industry-standard orchestrator.

### When Docker Compose is enough

- Single developer machine
- Team of < 5 with a single server
- Traffic is predictable and low

RetailStore currently uses Docker Compose. This guide will migrate it to Kubernetes once the concepts are clear.

---

## 2. Core Architecture

### 2.1 The Cluster

A **cluster** is a set of machines (physical or virtual) that Kubernetes manages as one.

```
┌─────────────────────────────────────────┐
│              Kubernetes Cluster          │
│  ┌──────────────┐   ┌────────────────┐  │
│  │  Control     │   │  Worker Node 1 │  │
│  │  Plane       │   │  (your app)    │  │
│  │              │   ├────────────────┤  │
│  │  • API Server│   │  Worker Node 2 │  │
│  │  • Scheduler │   │  (your app)    │  │
│  │  • etcd      │   ├────────────────┤  │
│  │  • Controller│   │  Worker Node N │  │
│  └──────────────┘   └────────────────┘  │
└─────────────────────────────────────────┘
```

**Control Plane components:**

| Component | Role |
|-----------|------|
| `kube-apiserver` | The single REST endpoint every `kubectl` command talks to |
| `etcd` | Distributed key-value store — the cluster's source of truth |
| `kube-scheduler` | Decides which Node a new Pod should run on |
| `kube-controller-manager` | Runs reconciliation loops (keeps desired state == actual state) |

**Worker Node components:**

| Component | Role |
|-----------|------|
| `kubelet` | Agent that ensures containers described in Pod specs are running |
| `kube-proxy` | Manages network rules for Service routing |
| Container runtime | containerd (most common) or Docker |

### 2.2 kubectl — The CLI

```bash
kubectl get pods                   # list pods in current namespace
kubectl describe pod <name>        # detailed info + events
kubectl logs <pod> -c <container>  # stream container logs
kubectl apply -f manifest.yaml     # declaratively apply a resource
kubectl delete -f manifest.yaml    # remove a resource
kubectl exec -it <pod> -- bash     # shell into a running container
```

### 2.3 Namespaces

Namespaces are virtual clusters inside one physical cluster. Use them for:

- **Environment isolation** — `retailstore-dev`, `retailstore-staging`, `retailstore-prod`
- **Team isolation** — each team gets a namespace with its own quotas

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: retailstore-prod
```

RetailStore will use namespaces per environment.

---

## 3. Workload Resources

### 3.1 Pod

The **smallest deployable unit**. A Pod wraps one or more containers that share:
- The same network namespace (same IP)
- The same storage volumes

Pods are **ephemeral** — they die and get replaced. You never manage raw Pods in production; you always use a controller.

```yaml
# You'll almost never write this in production:
apiVersion: v1
kind: Pod
metadata:
  name: backend
spec:
  containers:
  - name: api
    image: retailstore-backend:1.0
    ports:
    - containerPort: 8080
```

### 3.2 ReplicaSet

Ensures N copies of a Pod template are always running. If one dies, it creates a replacement. You rarely write ReplicaSets directly — Deployments manage them.

### 3.3 Deployment

The standard way to run **stateless** workloads (your backend API, your frontend Nginx).

Key capabilities:
- Declarative rollouts (rolling update, recreate)
- Rollback to a previous revision (`kubectl rollout undo`)
- Manages ReplicaSets for you

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: retailstore-backend
  namespace: retailstore-prod
spec:
  replicas: 3                      # run 3 copies
  selector:
    matchLabels:
      app: retailstore-backend
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1            # never bring more than 1 replica down at once
      maxSurge: 1                  # allow 1 extra replica during update
  template:
    metadata:
      labels:
        app: retailstore-backend
    spec:
      containers:
      - name: api
        image: retailstore-backend:1.0
        ports:
        - containerPort: 8080
```

**Deployment vs StatefulSet:**

| | Deployment | StatefulSet |
|---|---|---|
| Pod identity | Random names (`pod-abc`) | Stable ordinal names (`pod-0`, `pod-1`) |
| Storage | Shared or none | Each Pod gets its own PVC |
| Startup order | Parallel | Sequential |
| Use for | APIs, frontends | Databases, message brokers |

### 3.4 StatefulSet

Required for **stateful** workloads: databases, caches, message queues. Each Pod gets:
- A stable, predictable DNS name: `<name>-0.<service>.<namespace>.svc.cluster.local`
- Its own persistent volume that survives Pod restarts

SQL Server in RetailStore will use a StatefulSet.

### 3.5 DaemonSet

Runs exactly **one copy of a Pod on every Node**. Used for:
- Log collectors (Fluentd, Filebeat)
- Monitoring agents (Prometheus node-exporter)

RetailStore would use a DaemonSet for its log shipper in a production cluster.

### 3.6 Job & CronJob

- **Job** — runs a Pod to completion (database migrations, one-off scripts)
- **CronJob** — scheduled Jobs (nightly reports, cleanup tasks)

RetailStore's OutboxProcessor currently runs in-process. A CronJob could drive it as a separate concern.

---

## 4. Networking

### 4.1 Services

Pods have ephemeral IPs. A **Service** gives a stable IP and DNS name to a set of Pods, selected by labels.

**ClusterIP** (default) — internal only, not reachable from outside the cluster.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: retailstore-backend-svc
  namespace: retailstore-prod
spec:
  selector:
    app: retailstore-backend        # routes to all Pods with this label
  ports:
  - port: 80                        # Service port
    targetPort: 8080                # container port
  type: ClusterIP
```

**NodePort** — exposes a port on every Node's IP. Useful for development.

**LoadBalancer** — provisions an external cloud load balancer (AWS ALB, GCP Load Balancer). The standard way to expose services in the cloud.

**ExternalName** — maps a Service to an external DNS name.

### 4.2 Ingress

An **Ingress** is a Layer 7 (HTTP/HTTPS) router that sits in front of multiple Services. It provides:
- Host-based routing (`api.retailstore.com` → backend, `app.retailstore.com` → frontend)
- Path-based routing (`/api/*` → backend, `/*` → frontend)
- TLS termination

An **Ingress Controller** (NGINX Ingress, Traefik, AWS ALB Ingress) is the actual implementation.

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: retailstore-ingress
  namespace: retailstore-prod
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  ingressClassName: nginx
  rules:
  - host: retailstore.example.com
    http:
      paths:
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: retailstore-backend-svc
            port:
              number: 80
      - path: /
        pathType: Prefix
        backend:
          service:
            name: retailstore-frontend-svc
            port:
              number: 80
  tls:
  - hosts:
    - retailstore.example.com
    secretName: retailstore-tls
```

### 4.3 DNS inside the cluster

Every Service automatically gets a DNS name:
`<service-name>.<namespace>.svc.cluster.local`

So the backend can reach the database at:
`retailstore-sqlserver-svc.retailstore-prod.svc.cluster.local`

This is the Kubernetes equivalent of how `backend` container reaches `db` container in Docker Compose via the `retailstore_net` bridge network.

### 4.4 NetworkPolicy

Firewall rules for Pod-to-Pod traffic. By default all Pods can talk to all other Pods. NetworkPolicies restrict this.

```yaml
# Only allow the backend to reach the database
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-backend-to-db
  namespace: retailstore-prod
spec:
  podSelector:
    matchLabels:
      app: retailstore-sqlserver
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: retailstore-backend
    ports:
    - port: 1433
```

---

## 5. Configuration & Secrets

### 5.1 ConfigMap

Non-sensitive key-value configuration. Mounted as environment variables or files.

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: retailstore-config
  namespace: retailstore-prod
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Jwt__Issuer: "RetailStoreAPI"
  Jwt__Audience: "RetailStoreClient"
  Jwt__ExpiryInMinutes: "30"
  Seed__AdminEmail: "admin@retailstore.com"
```

### 5.2 Secret

Base64-encoded sensitive data. **Never commit Secrets to git** — use tools like Sealed Secrets, External Secrets Operator, or HashiCorp Vault.

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: retailstore-secrets
  namespace: retailstore-prod
type: Opaque
data:
  DB_PASSWORD: ViFyS..."   # base64 of actual password
  JWT_SECRET: ZEs4WXZ...  # base64 of actual JWT secret
```

Create from command line (avoids putting values in YAML):
```bash
kubectl create secret generic retailstore-secrets \
  --from-literal=DB_PASSWORD='V!8rK#2mQ@7xLp$4Nz' \
  --from-literal=JWT_SECRET='dK8Yv...' \
  --namespace=retailstore-prod
```

### 5.3 Injecting config into containers

```yaml
# In a Deployment's container spec:
envFrom:
- configMapRef:
    name: retailstore-config      # all keys become env vars
- secretRef:
    name: retailstore-secrets     # all keys become env vars
env:
- name: ConnectionStrings__DefaultConnection
  value: "Server=retailstore-sqlserver-svc;Database=RetailStoreDb;User Id=sa;Password=$(DB_PASSWORD);TrustServerCertificate=True;"
```

**In Docker Compose** you use `.env` files and `environment:` blocks. In Kubernetes the equivalent is ConfigMaps + Secrets injected via `envFrom` / `env`.

---

## 6. Storage

### 6.1 Volumes

A **Volume** is a directory accessible to containers in a Pod. Types:

| Type | Description |
|------|-------------|
| `emptyDir` | Temporary, lives with the Pod (cache, scratch space) |
| `hostPath` | Mounts a path from the Node's filesystem (avoid in production) |
| `configMap` / `secret` | Mounts config data as files |
| `persistentVolumeClaim` | Requests durable storage from the cluster |

### 6.2 PersistentVolume (PV) & PersistentVolumeClaim (PVC)

**PV** — a piece of storage provisioned by an admin or dynamically by a StorageClass (e.g., an AWS EBS volume, Azure Disk, or a local SSD).

**PVC** — a request for storage by a workload. Kubernetes binds the PVC to a suitable PV.

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: sqlserver-data
  namespace: retailstore-prod
spec:
  accessModes:
  - ReadWriteOnce               # one node can mount it read-write
  storageClassName: standard    # maps to a StorageClass in your cluster
  resources:
    requests:
      storage: 20Gi
```

**In Docker Compose** you defined `volumes: sqlserver_data:`. In Kubernetes the equivalent is a PVC bound to a PV — the data survives Pod restarts and rescheduling.

### 6.3 StorageClass

Defines *how* storage is provisioned (which provisioner, which disk type). Cloud providers have built-in StorageClasses:
- AWS: `gp2`, `gp3`
- GCP: `standard`, `premium-rwo`
- Azure: `default`, `managed-premium`
- Local (dev): `local-path` (via Rancher's provisioner)

---

## 7. Health, Scheduling & Resource Management

### 7.1 Probes

Kubernetes needs to know if a container is alive and ready to serve traffic.

**LivenessProbe** — if it fails, Kubernetes *restarts* the container.
**ReadinessProbe** — if it fails, Kubernetes *removes the Pod from Service endpoints* (stops sending traffic).
**StartupProbe** — gives slow-starting containers extra time before liveness kicks in. Essential for SQL Server.

```yaml
# In the backend container spec:
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 15
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10
  failureThreshold: 3
```

RetailStore already has health checks (`OutboxHealthCheck`, SQL Server health check). These map directly to `/health` endpoints used by Kubernetes probes.

### 7.2 Resource Requests & Limits

**Request** — the minimum resources guaranteed to a container. The scheduler uses this to place the Pod.
**Limit** — the maximum a container can use. Exceeding CPU limit throttles it; exceeding memory limit kills it (OOMKilled).

```yaml
resources:
  requests:
    cpu: "250m"       # 250 millicores = 0.25 cores
    memory: "256Mi"
  limits:
    cpu: "1000m"      # 1 full core max
    memory: "512Mi"
```

Always set both. Without limits, one runaway container can starve the whole node.

### 7.3 HorizontalPodAutoscaler (HPA)

Automatically scales the number of replicas based on CPU, memory, or custom metrics.

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: retailstore-backend-hpa
  namespace: retailstore-prod
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: retailstore-backend
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70    # scale up when avg CPU > 70%
```

### 7.4 Node Affinity & Taints/Tolerations

**Node Affinity** — schedule a Pod preferably (or only) on certain Nodes (e.g., high-memory nodes for SQL Server).

**Taints & Tolerations** — a Node can be "tainted" to repel Pods. Only Pods with a matching "toleration" can be scheduled there. Used to dedicate nodes to specific workloads.

**PodDisruptionBudget (PDB)** — guarantees a minimum number of Pods stay running during voluntary disruptions (node drains, cluster upgrades).

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: retailstore-backend-pdb
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: retailstore-backend
```

---

## 8. Observability

### 8.1 Logs

Kubernetes captures stdout/stderr from every container. Logs are accessible via:
```bash
kubectl logs <pod> -c <container> --follow
```

For production, ship logs to a centralized system with a **DaemonSet** log forwarder:
- **Fluent Bit** (lightweight) → OpenSearch / Elasticsearch / Loki
- **Loki** (Grafana's log aggregation) — pairs well with the RetailStore's Serilog structured JSON logs

RetailStore uses Serilog. Configure it to output JSON to stdout and Kubernetes + Fluent Bit handles the rest.

### 8.2 Metrics

**Prometheus** is the standard metrics backend for Kubernetes. It scrapes `/metrics` endpoints from Pods. **Grafana** visualizes them.

RetailStore uses OpenTelemetry, which can export metrics in Prometheus format — zero additional instrumentation needed.

```yaml
# Annotate the backend Pod so Prometheus discovers it:
annotations:
  prometheus.io/scrape: "true"
  prometheus.io/port: "8080"
  prometheus.io/path: "/metrics"
```

### 8.3 Tracing

Distributed tracing (OpenTelemetry → Jaeger or Tempo) lets you follow a request across multiple services. RetailStore already instruments OpenTelemetry — deploying a **Jaeger** or **Grafana Tempo** instance in the cluster completes the stack.

### 8.4 The Full Observability Stack (for RetailStore)

```
Logs   → Serilog JSON stdout → Fluent Bit DaemonSet → Loki → Grafana
Metrics → OpenTelemetry      → Prometheus             → Grafana
Traces  → OpenTelemetry      → Jaeger / Tempo         → Grafana
```

---

## 9. Security

### 9.1 RBAC (Role-Based Access Control)

Controls who (human or service) can do what to which Kubernetes resources.

- **Role** — permissions scoped to a namespace
- **ClusterRole** — permissions cluster-wide
- **RoleBinding** / **ClusterRoleBinding** — binds a Role to a user or ServiceAccount

### 9.2 ServiceAccount

Every Pod runs with a ServiceAccount identity. By default it's the `default` account. Create dedicated accounts with minimal permissions.

### 9.3 PodSecurity Standards (PSS)

Kubernetes enforces security profiles on Pods at the namespace level:
- `privileged` — no restrictions (avoid)
- `baseline` — prevents known privilege escalations
- `restricted` — heavily restricted (recommended for production)

### 9.4 Secrets Management Best Practices

| Approach | Description |
|----------|-------------|
| Kubernetes Secrets | Fine for dev; base64 is NOT encryption |
| Sealed Secrets (Bitnami) | Encrypts Secrets so they're safe to commit to git |
| External Secrets Operator | Pulls secrets from AWS Secrets Manager, Azure Key Vault, HashiCorp Vault |
| HashiCorp Vault | Full secrets lifecycle management |

For RetailStore: start with Sealed Secrets or External Secrets Operator so the `.env` file never enters the cluster plain-text.

---

## 10. Helm: The Package Manager

**Helm** is to Kubernetes what `apt` is to Ubuntu. It packages Kubernetes manifests into **Charts** that are:
- Templated (reuse manifests across environments with different values)
- Versioned and releasable
- Shareable on [Artifact Hub](https://artifacthub.io/)

### Chart structure

```
retailstore/              # Chart root
├── Chart.yaml            # metadata (name, version, appVersion)
├── values.yaml           # default configuration values
├── values-dev.yaml       # environment overrides
├── values-prod.yaml      # environment overrides
└── templates/
    ├── deployment-backend.yaml
    ├── deployment-frontend.yaml
    ├── statefulset-sqlserver.yaml
    ├── service-backend.yaml
    ├── service-frontend.yaml
    ├── service-sqlserver.yaml
    ├── ingress.yaml
    ├── configmap.yaml
    ├── hpa.yaml
    └── pdb.yaml
```

```yaml
# values.yaml
backend:
  image: retailstore-backend
  tag: "1.0"
  replicas: 2
  resources:
    requests: { cpu: "250m", memory: "256Mi" }
    limits:   { cpu: "1",    memory: "512Mi" }

frontend:
  image: retailstore-frontend
  tag: "1.0"
  replicas: 2

sqlserver:
  storage: "20Gi"
  storageClass: "standard"
```

```bash
helm install retailstore ./retailstore -f values-prod.yaml -n retailstore-prod
helm upgrade retailstore ./retailstore -f values-prod.yaml -n retailstore-prod
helm rollback retailstore 1               # roll back to revision 1
```

---

## 11. CI/CD with Kubernetes

### Typical pipeline stages

```
Push to main branch
  → Build Docker images
  → Tag images with git SHA
  → Push to container registry (Docker Hub, ECR, GCR, ACR)
  → Run tests (docker-compose.test.yml or a K8s Job)
  → Helm upgrade on staging cluster
  → Run smoke tests / E2E
  → Helm upgrade on production cluster (with approval gate)
```

### GitOps with ArgoCD / Flux

Instead of running `helm upgrade` in a CI script, **GitOps** tools watch a git repository and automatically sync the cluster to whatever state is declared in git. This means:
- All cluster changes are code-reviewed PRs
- Rollback = `git revert`
- Audit trail is git history

**ArgoCD** is the most common tool. It monitors a Helm chart repository and keeps the cluster in sync.

---

## 12. What Was Actually Built

This section documents every file that was created or modified to bring RetailStore onto Kubernetes, what each one does, and why.

---

### Files Created

#### `k8s/` — Raw Kubernetes manifests (reference / phase-by-phase history)

These files were the working scratch-pad for each phase. The Helm chart (below) is the canonical source of truth going forward — the raw files remain for reference.

| File | K8s Kind | What it does |
|------|----------|--------------|
| `k8s/namespaces.yaml` | Namespace | Creates `retailstore-dev` and `retailstore-prod` isolation boundaries |
| `k8s/configmap.yaml` | ConfigMap | All non-secret config: `ASPNETCORE_ENVIRONMENT`, JWT settings, `Seed__AdminEmail` |
| `k8s/sqlserver.yaml` | StatefulSet + Service | SQL Server 2022 with a 10 Gi PVC; ClusterIP service on port 1433 |
| `k8s/backend.yaml` | Deployment + Service | 2-replica .NET API; rolling update; initContainer waits for SQL Server |
| `k8s/frontend.yaml` | Deployment + Service | 2-replica Nginx serving the React SPA |
| `k8s/ingress.yaml` | Ingress | Catch-all NGINX ingress: `/api/*` → backend, `/*` → frontend |
| `k8s/hpa.yaml` | HPA + PDB | Auto-scales backend 2→8 replicas at 70% CPU; PDB keeps ≥1 pod alive during drains |
| `k8s/migrate-job.yaml` | Job | Runs `dotnet RetailStore.Api.dll --migrate` once to create the DB schema + seed data |

#### `k8s/helm/retailstore/` — The Helm Chart (canonical deploy artifact)

```
k8s/helm/retailstore/
├── Chart.yaml             # name: retailstore, appVersion: "2.0"
├── values.yaml            # dev defaults
├── values-prod.yaml       # production overrides
└── templates/
    ├── _helpers.tpl        # common label helper used by every template
    ├── configmap.yaml      # parameterized from values.config.*
    ├── secret.yaml         # DB_PASSWORD + JWT_SECRET via b64enc
    ├── sqlserver.yaml      # StatefulSet + Service; storage size from values
    ├── backend.yaml        # Deployment + Service; replicas, image tag, resources from values
    ├── frontend.yaml       # Deployment + Service
    ├── ingress.yaml        # conditional host: empty = catch-all, set = hostname-restricted
    ├── hpa.yaml            # conditional: only rendered when backend.hpa.enabled=true
    ├── pdb.yaml            # minAvailable from values
    └── migrate-job.yaml    # Helm post-install/post-upgrade hook — runs after SQL Server is up
```

**Key values that differ between environments:**

| Value | Dev (`values.yaml`) | Prod (`values-prod.yaml`) |
|-------|---------------------|---------------------------|
| `namespace` | `retailstore-dev` | `retailstore-prod` |
| `backend.replicas` | 2 | 3 |
| `backend.image.pullPolicy` | `IfNotPresent` | `Always` |
| `backend.hpa.minReplicas` | 2 | 3 |
| `backend.hpa.maxReplicas` | 8 | 12 |
| `sqlserver.storage` | `10Gi` | `50Gi` |
| `ingress.host` | `""` (catch-all) | `"retailstore.com"` |
| `config.aspnetcoreEnvironment` | `Development` | `Production` |

#### `.github/workflows/ci-cd.yml` — GitHub Actions Pipeline

Four jobs that run in order:

```
push to master
      │
      ├─► test-backend    dotnet test (SQLite in-memory — no SQL Server needed)
      ├─► test-frontend   tsc --noEmit + vitest run
      │         ↓ (both must pass)
      └─► build-push      Build + push images to ghcr.io with git SHA tag
                │         (only on master push, not on PRs)
                ↓
            deploy         helm upgrade --install --atomic
                           kubectl rollout status (verify pods healthy)
```

---

### Files Modified

#### `src/RetailStore.Api/Program.cs`

Added a `--migrate` mode at the very top, before any web server setup:

```csharp
if (args.Contains("--migrate"))
{
    // Minimal DI container — no Kestrel, no hosted services, no background threads.
    // Connects to SQL Server, runs EnsureCreated() + seeds roles and admin user, exits 0.
    var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(config);
    services.AddSingleton(new DbContextAssemblyOptions { ... });
    services.AddDbContext<RetailStoreDbContext>(o => o.UseSqlServer(...));
    services.AddLogging(b => b.AddConsole());
    await using var sp = services.BuildServiceProvider();
    await DatabaseSeeder.SeedAsync(sp, sp.GetRequiredService<ILogger<Program>>());
    return;   // process exits — no HTTP server started
}
```

**Why a plain `ServiceCollection` and not `WebApplication.CreateBuilder`?** `WebApplication.CreateBuilder` registers infrastructure (config watchers, `IHostLifetime`) that spawns non-background threads, keeping the process alive even after `return`. A bare `ServiceCollection` has no such threads — the process exits immediately after `SeedAsync` completes.

#### `retail-store-frontend/nginx.conf`

Changed the API upstream from the Docker Compose service name to the Kubernetes Service DNS name:

```nginx
# Before (Docker Compose):
proxy_pass http://backend:8080/api/;

# After (Kubernetes):
proxy_pass http://retailstore-backend-svc/api/;
# Port omitted because the K8s Service maps 80 → 8080 internally.
```

#### `retail-store-frontend/tsconfig.app.json`

Added `exclude` so `tsc -b` (run during `docker build`) ignores test files:

```json
"exclude": ["src/**/__tests__/**", "src/**/*.test.ts", "src/**/*.test.tsx"]
```

Without this, test files that reference removed toast-store properties caused the production Docker build to fail with TypeScript errors.

#### Several frontend source files

Five unused-import / unused-variable TypeScript errors were fixed to make the strict `tsc -b` build pass:
- `router/index.tsx` — removed dead `PlaceholderPage` function and its `useTranslation` import
- `NotificationDetailPanel.tsx`, `PreferencesModal.tsx` — removed `useTranslation` imports where `t` was never called
- `OrderDetailPanel.tsx` — removed unused `DollarSign` lucide import
- `OrdersListPage.tsx` — removed unused `XCircle` lucide import
- `PaymentDetailPanel.tsx` — renamed unused `paymentId` parameter to `_paymentId`

---

### Infrastructure Added to the Cluster

| Resource | Namespace | Why |
|----------|-----------|-----|
| NGINX Ingress Controller | `ingress-nginx` | Single entry point for all HTTP traffic |
| metrics-server | `kube-system` | Required by HPA to read CPU utilization |
| `retailstore-sqlserver` StatefulSet | `retailstore-dev` | Persistent SQL Server with its own PVC |
| `retailstore-backend` Deployment (2 replicas) | `retailstore-dev` | The .NET API, rolling-update capable |
| `retailstore-frontend` Deployment (2 replicas) | `retailstore-dev` | Nginx serving the React SPA |
| `retailstore-backend-hpa` | `retailstore-dev` | Auto-scales 2→8 replicas on CPU |
| `retailstore-backend-pdb` | `retailstore-dev` | Keeps ≥1 backend pod alive during node drains |
| Helm release `retailstore` | `retailstore-dev` | Helm now owns all of the above resources |

---

## 13. How the App Benefits from This

### Before (Docker Compose)

| Problem | Impact |
|---------|--------|
| Single process per service | One crash kills the whole API |
| Manual restarts on failure | Downtime until someone notices |
| No traffic control during deploys | Users hit "502 Bad Gateway" during image replacement |
| Scale = manual `docker compose scale` | Requires SSH + human decision |
| Secrets in `.env` files | Easy to leak via git or shared drives |
| Port conflicts on the host | Frontend on 3000, backend on 5240 — coupled to machine |

### After (Kubernetes)

| Benefit | How it works in this project |
|---------|------------------------------|
| **Self-healing** | `restartPolicy: Always` on both Deployments. A crashed pod is replaced within seconds — no human needed. |
| **Zero-downtime deploys** | `RollingUpdate` with `maxUnavailable: 1`. Kubernetes brings up a new pod, waits for its readiness probe to pass, only then kills the old one. Users never see a gap. |
| **Automatic scaling** | The HPA scales the backend from 2 to 8 replicas when average CPU exceeds 70%. On a quiet night it scales back down. |
| **Traffic-aware routing** | The readiness probe (`/health/ready`) checks both SQL Server connectivity and Outbox queue depth. Kubernetes only sends traffic to pods that pass. A pod re-connecting to SQL Server is automatically drained before Kubernetes removes it from the pool. |
| **Maintenance safety** | The PDB (`minAvailable: 1`) prevents Kubernetes from evicting both backend pods simultaneously during node upgrades. You always have at least one pod serving traffic. |
| **Migration safety** | The Helm post-install hook runs `EnsureCreated` + seeding exactly once, as its own isolated process, before any backend traffic is served. Multiple pod replicas starting at the same time no longer race to create the schema. |
| **Environment parity** | `values.yaml` and `values-prod.yaml` are the only difference between dev and production. The same Helm chart, the same images, the same manifests — just different knobs. |
| **Audit trail** | Every deploy is a Helm revision. `helm history retailstore -n retailstore-dev` shows every deploy with its timestamp. `helm rollback retailstore 2` rolls back in seconds. |
| **Secret hygiene** | Database password and JWT key live in a Kubernetes Secret, not in a `.env` file on disk. In CI/CD they come from GitHub Secrets — never written to any file. |

---

## 14. Running in Development (Local)

### Prerequisites

```powershell
# 1. Enable Kubernetes in Docker Desktop (Settings → Kubernetes → Enable Kubernetes)
#    OR: winget install minikube && minikube start

# 2. Install Helm (if not already installed)
winget install Helm.Helm

# 3. Install NGINX Ingress Controller (one-time cluster setup)
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx --create-namespace

# 4. Install metrics-server + patch for Docker Desktop TLS (one-time cluster setup)
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
kubectl patch deployment metrics-server -n kube-system --type=json \
  -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'
```

### Build local images

```powershell
# Build backend (run from repo root)
docker build -t retailstore-backend:2.0 .

# Build frontend
docker build -t retailstore-frontend:1.0 retail-store-frontend/
```

> **Important — image tag caching:** Docker Desktop's Kubernetes caches images in containerd separately from Docker. If you rebuild with the same tag (e.g., `2.0`), the cluster may still use the old image. Always bump the tag (`2.1`, `2.2`, …) or use `imagePullPolicy: Always` with a local registry. In CI/CD this is handled automatically by the git SHA tag.

### Deploy with Helm

```powershell
helm upgrade --install retailstore k8s/helm/retailstore `
  --namespace retailstore-dev `
  --create-namespace `
  --timeout 8m `
  --set secrets.dbPassword="YourStrong@Passw0rd" `
  --set secrets.jwtSecret="your-super-secret-jwt-key-for-retail-store-app"
```

On first install, Helm will:
1. Create the `retailstore-dev` namespace
2. Deploy ConfigMap, Secret, SQL Server, backend, frontend, ingress, HPA, PDB
3. Run the post-install migrate Job (schema creation + seeding)

Subsequent runs (`helm upgrade --install`) detect the existing release and apply only the diff.

### Access the app in the browser

```powershell
# In a terminal (keep it open while using the browser):
kubectl port-forward svc/ingress-nginx-controller 8080:80 -n ingress-nginx
```

Then open: **`http://localhost:8080`**

The request flow:
```
Browser → localhost:8080
  → port-forward → NGINX Ingress Controller
    → /api/* → retailstore-backend-svc:80 → .NET API pod
    → /*     → retailstore-frontend-svc:80 → Nginx pod → index.html
```

> If you want a proper hostname (`http://retailstore.local`) instead of `localhost:8080`, add the following line to `C:\Windows\System32\drivers\etc\hosts` as Administrator:
> ```
> 127.0.0.1 retailstore.local
> ```
> Then set `ingress.host=retailstore.local` in your Helm install command and access via `http://retailstore.local:8080`.

### When you change backend code

```powershell
# 1. Build a new image with a new tag
docker build -t retailstore-backend:2.1 .

# 2. Upgrade the Helm release — Helm diffs and only rolls out what changed
helm upgrade retailstore k8s/helm/retailstore `
  --namespace retailstore-dev `
  --set backend.image.tag=2.1 `
  --set secrets.dbPassword="YourStrong@Passw0rd" `
  --set secrets.jwtSecret="your-super-secret-jwt-key-for-retail-store-app"
```

Helm will trigger a rolling update on the backend Deployment only. SQL Server and frontend are untouched.

### Tearing down

```powershell
# Remove everything managed by Helm (keeps the namespace, removes all app resources)
helm uninstall retailstore -n retailstore-dev

# Remove the namespace entirely
kubectl delete namespace retailstore-dev
```

---

## 15. Deploying to Production via GitHub

### How the pipeline works

Every push to `master` triggers `.github/workflows/ci-cd.yml`:

1. **`test-backend`** — `dotnet test` using SQLite in-memory. No SQL Server needed; runs fast.
2. **`test-frontend`** — TypeScript check + `vitest run` unit tests.
3. **`build-push`** (only if both tests pass, only on `master`) — builds both Docker images and pushes to `ghcr.io/fernandojvela/` with two tags:
   - `ghcr.io/fernandojvela/retailstore-backend:<git-sha>` — immutable, used for the deploy
   - `ghcr.io/fernandojvela/retailstore-backend:latest` — mutable, used as build cache for the next run
4. **`deploy`** — runs `helm upgrade --install --atomic` pointing at the new `<git-sha>` tag.
   - `--atomic`: if pods fail to become ready, Helm automatically rolls back to the previous release.
   - `kubectl rollout status` confirms both Deployments are fully healthy before the job exits green.

### One-time setup

#### 1. Create the GitHub Secrets

Go to **Settings → Secrets and variables → Actions → New repository secret** and add:

| Secret name | Value |
|-------------|-------|
| `DB_PASSWORD` | Your production SQL Server SA password |
| `JWT_SECRET` | Your production JWT signing key (≥32 characters) |
| `KUBECONFIG_DEV` | Base64-encoded kubeconfig for your cluster (see below) |

To generate `KUBECONFIG_DEV` from your cluster machine:

```bash
# On a machine with kubectl access to the cluster:
kubectl config view --raw | base64 -w0
# Copy the output and paste it as the KUBECONFIG_DEV secret value
```

For cloud clusters:
```bash
# AKS (Azure)
az aks get-credentials --resource-group <rg> --name <cluster>
kubectl config view --raw | base64 -w0

# GKE (Google Cloud)
gcloud container clusters get-credentials <cluster> --region <region>
kubectl config view --raw | base64 -w0

# EKS (AWS)
aws eks update-kubeconfig --name <cluster> --region <region>
kubectl config view --raw | base64 -w0
```

#### 2. Create the GitHub Environment

Go to **Settings → Environments → New environment**, name it `dev`. This unlocks:
- **Protection rules** — require a manual approval before deploying (recommended for production)
- **Environment secrets** — secrets scoped to this environment only
- **Deploy history** — a per-environment log of every deploy

#### 3. Make the images public (or configure pull access)

GitHub Container Registry images are private by default. Either:
- Make the packages public: **Your profile → Packages → retailstore-backend → Package settings → Change visibility → Public**
- Or add an `imagePullSecret` to the Helm chart pointing at a `ghcr.io` registry credential

#### 4. Pre-create the production namespace + SQL Server (first deploy only)

SQL Server needs to exist and be healthy before the Helm post-install migrate hook can run. On the very first deploy to a new cluster:

```bash
# On the cluster machine, or via kubectl with kubeconfig set:
kubectl create namespace retailstore-prod

# If SQL Server needs to be pre-seeded with a persistent volume:
helm upgrade --install retailstore k8s/helm/retailstore \
  -f k8s/helm/retailstore/values-prod.yaml \
  --namespace retailstore-prod \
  --create-namespace \
  --timeout 10m \
  --set secrets.dbPassword="<prod-password>" \
  --set secrets.jwtSecret="<prod-jwt-key>"
```

After that first manual deploy, all subsequent deploys run automatically via GitHub Actions.

### What a typical deploy looks like

```
git push origin master

GitHub Actions:
  ✓ test-backend       (≈ 2 min)
  ✓ test-frontend      (≈ 1 min)
  ✓ build-push         (≈ 4 min — layers cached from last push)
  ✓ deploy             (≈ 3 min — helm upgrade + rollout status)

Total: ~ 10 minutes from push to live
```

If the deploy fails (readiness probe never passes), `--atomic` rolls back automatically — no manual intervention needed.

### Rollback

```bash
# See all releases
helm history retailstore -n retailstore-dev

# Roll back to revision N
helm rollback retailstore <N> -n retailstore-dev
```

---

## 16. Day-to-Day Operations Reference

### Checking cluster health

```powershell
# Everything in the namespace
kubectl get all -n retailstore-dev

# Pod logs (live stream)
kubectl logs -f deployment/retailstore-backend -n retailstore-dev

# Events (useful when a pod won't start)
kubectl describe pod -n retailstore-dev <pod-name>

# Current CPU/memory usage per pod
kubectl top pods -n retailstore-dev

# HPA status (current CPU vs threshold)
kubectl get hpa -n retailstore-dev

# Helm release history
helm history retailstore -n retailstore-dev
```

### Accessing the running API inside the cluster

```powershell
# Forward the backend service to your machine (for Swagger, debugging)
kubectl port-forward svc/retailstore-backend-svc 8080:80 -n retailstore-dev
# Then open: http://localhost:8080/swagger

# Shell into a running backend pod
kubectl exec -it deployment/retailstore-backend -n retailstore-dev -- bash
```

### Manually running the migrate Job

Useful if you need to re-seed data or run `EnsureCreated` after a schema change:

```powershell
# Delete the previous completed job (Jobs are immutable once applied)
kubectl delete job retailstore-migrate -n retailstore-dev --ignore-not-found

# Apply and wait
kubectl apply -f k8s/migrate-job.yaml
kubectl wait --for=condition=complete job/retailstore-migrate -n retailstore-dev --timeout=120s

# Check the logs
kubectl logs job/retailstore-migrate -n retailstore-dev
```

### Scaling manually

```powershell
# Temporarily override replicas (HPA will regain control on the next metrics cycle)
kubectl scale deployment retailstore-backend --replicas=4 -n retailstore-dev

# Suspend HPA (to keep manual replica count stable)
kubectl patch hpa retailstore-backend-hpa -n retailstore-dev \
  -p '{"spec":{"minReplicas":4,"maxReplicas":4}}'
```

### Updating a config value without rebuilding the image

```powershell
# Example: change the JWT expiry time
helm upgrade retailstore k8s/helm/retailstore `
  --namespace retailstore-dev `
  --reuse-values `
  --set config.jwtExpiryMinutes=60 `
  --set secrets.dbPassword="YourStrong@Passw0rd" `
  --set secrets.jwtSecret="your-super-secret-jwt-key-for-retail-store-app"
```

Helm updates only the ConfigMap. The backend pods are not restarted automatically — you need to trigger a rollout:

```powershell
kubectl rollout restart deployment/retailstore-backend -n retailstore-dev
```

### Key lessons learned during implementation

| Issue | Root cause | Fix |
|-------|-----------|-----|
| `--migrate` mode didn't exit | `WebApplication.CreateBuilder` registers background infrastructure that keeps the process alive | Use a bare `ServiceCollection` in migrate mode — no web host, no background threads |
| K8s used old image after rebuild | `imagePullPolicy: IfNotPresent` checks by tag name, not digest. Docker Desktop's containerd and Docker daemon have separate caches | Always bump the image tag when rebuilding locally. In CI/CD the git SHA tag handles this automatically |
| Ingress not reachable from Windows | Docker Desktop's kind cluster puts the LoadBalancer IP on Docker's internal network (`172.x.x.x`), not on the Windows host | Use `kubectl port-forward svc/ingress-nginx-controller 8080:80` instead of trying to reach the LoadBalancer IP directly |
| Helm pre-install hook failed (namespace not ready) | The migrate Job ran before the namespace existed | Moved the migrate Job to a `post-install,post-upgrade` hook; SQL Server is deployed first as a main template |
| Namespace stuck in Terminating | Helm's `before-hook-creation` delete policy on a namespace hook triggered a delete-then-recreate cycle | Removed the namespace from Helm templates; use `--create-namespace` flag instead |

---

## 17. Docker Compose → Kubernetes Mapping

| Docker Compose concept | Kubernetes equivalent | In this project |
|------------------------|----------------------|-----------------|
| `services.db` | StatefulSet + PVC + Service | `retailstore-sqlserver` StatefulSet with 10 Gi PVC |
| `services.backend` | Deployment + Service | `retailstore-backend` Deployment, 2 replicas |
| `services.frontend` | Deployment + Service | `retailstore-frontend` Deployment, 2 replicas |
| Port mapping `5240:8080` | Ingress + Service | NGINX Ingress + `retailstore-backend-svc` |
| Port mapping `3000:80` | Ingress + Service | NGINX Ingress + `retailstore-frontend-svc` |
| `volumes: sqlserver_data` | PersistentVolumeClaim | `sqlserver-data` PVC, `standard` StorageClass |
| `.env` file | ConfigMap + Secret | `retailstore-config` ConfigMap + `retailstore-secrets` Secret |
| `environment:` block | `envFrom` + `env` in container spec | `envFrom: configMapRef` + individual `secretKeyRef` |
| `depends_on: db` | InitContainer | `wait-for-sqlserver` initContainer polls `nc -zw1 retailstore-sqlserver-svc 1433` |
| `networks: retailstore_net` | Cluster networking | All pods share a flat network; service DNS resolves within the namespace |
| `restart: unless-stopped` | `restartPolicy: Always` | Default for Deployment pods |
| `docker-compose scale backend=3` | `kubectl scale` or HPA | HPA auto-scales 2→8 at 70% CPU |
| `docker-compose.test.yml` | Kubernetes Job / CI `dotnet test` | `dotnet test` in GitHub Actions; migrate Job for DB init |
| `docker-compose.yml` (whole file) | Helm chart | `k8s/helm/retailstore/` with `values.yaml` + `values-prod.yaml` |
