# ═══════════════════════════════════════════════════════════════════════════════
# RetailStore Backend — Multi-Stage Dockerfile
#
# THEORY: Multi-stage builds let us use a large SDK image to compile the code and
# then copy only the compiled output into a tiny runtime image. The final image
# does not contain the .NET SDK, source code, or test frameworks — only what is
# needed to run the application.
#
# HOW LAYER CACHING WORKS:
#   Docker executes each instruction and stores the result as an immutable layer.
#   If a layer's inputs haven't changed, Docker reuses the cached layer instead of
#   re-executing it. The trick below is to copy .csproj files first and run
#   "dotnet restore" before copying the actual source code. That way, NuGet packages
#   are only re-downloaded when a .csproj file changes — not on every code change.
# ═══════════════════════════════════════════════════════════════════════════════

# ─── Stage 1: build ──────────────────────────────────────────────────────────
# Uses the full .NET SDK image — includes the compiler, CLI tools, and NuGet.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Step 1a: Copy ONLY the project files.
# This layer is cached until a .csproj changes (rare), so "dotnet restore" below
# is skipped on code-only changes — dramatically faster rebuilds.
COPY src/RetailStore.Api/RetailStore.Api.csproj                        src/RetailStore.Api/
COPY src/RetailStore.Infrastructure/RetailStore.Infrastructure.csproj  src/RetailStore.Infrastructure/
COPY src/RetailStore.SharedKernel/RetailStore.SharedKernel.csproj      src/RetailStore.SharedKernel/
COPY tests/RetailStore.Tests/RetailStore.Tests.csproj                  tests/RetailStore.Tests/

# Step 1b: Restore NuGet packages for all projects.
# The test project references all src projects, so one restore covers the whole solution.
RUN dotnet restore tests/RetailStore.Tests/RetailStore.Tests.csproj

# Step 1c: Copy the full source code now that packages are cached.
COPY src/ src/
COPY tests/ tests/

# ─── Stage 2: test ───────────────────────────────────────────────────────────
# Inherits all layers from the build stage (packages + source).
# "dotnet test" builds the project and then runs every test in the suite.
#
# IMPORTANT: All tests — including integration tests — use SQLite in-memory
# (see RetailStoreWebAppFactory.cs). No external SQL Server is required here.
# A non-zero exit code from the tests causes this stage to fail, which
# prevents a broken image from being tagged as "runtime".
FROM build AS test
RUN dotnet test tests/RetailStore.Tests/RetailStore.Tests.csproj \
    --logger "console;verbosity=normal"

# ─── Stage 3: publish ────────────────────────────────────────────────────────
# "dotnet publish" compiles a Release build and bundles all DLLs, config files,
# and native binaries into a single output directory (/app/publish).
# --no-restore reuses the packages already downloaded in Stage 1.
FROM build AS publish
RUN dotnet publish src/RetailStore.Api/RetailStore.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ─── Stage 4: runtime ────────────────────────────────────────────────────────
# Uses the tiny ASP.NET Core runtime image — no SDK, no compiler, no source code.
# This is the image that actually gets deployed / run.
#
# ASPNETCORE_URLS: tells the Kestrel web server to listen on HTTP port 8080.
#   We disable HTTPS here because TLS should be terminated by a reverse proxy
#   (Nginx, a load balancer, or a cloud provider's edge) — not by the app itself.
#   Running HTTP inside the container is safe because it never leaves the Docker
#   network unencrypted.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RetailStore.Api.dll"]
