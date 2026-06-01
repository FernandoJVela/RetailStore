# ═══════════════════════════════════════════════════════════════════════════════
# RetailStore Frontend — Multi-Stage Dockerfile
#
# STAGES:
#   deps    — install npm packages (cached layer, reused by all stages below)
#   dev     — run Vite dev server with hot-module replacement (development)
#   test    — run Vitest unit tests and exit (CI / docker-compose.test.yml)
#   build   — compile TypeScript and bundle assets (production)
#   runtime — Nginx serving the compiled bundle (production)
#
# WHY NODE:22-ALPINE?
#   "alpine" Linux images are based on musl libc and busybox. They are ~5 MB
#   compared to ~200 MB for debian-based images. For a Node.js build step,
#   this is ideal — we only need Node.js temporarily to compile; we don't need
#   a full Linux toolchain in the final image.
# ═══════════════════════════════════════════════════════════════════════════════

# ─── Stage 1: deps ───────────────────────────────────────────────────────────
# Copy only package.json and package-lock.json first (same caching trick as .NET).
# "npm ci" installs the exact versions locked in package-lock.json.
# This layer is cached until a package file changes — code changes don't
# invalidate the install layer.
FROM node:22-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

# ─── Stage 2: dev ────────────────────────────────────────────────────────────
# Development mode: runs the Vite dev server with hot-module replacement (HMR).
# Source code is copied at image build time, but in practice you would use a
# volume mount in docker-compose.yml so live edits are reflected without rebuilding.
#
# "--host" (or equivalently, server.host: true in vite.config.ts) binds Vite
# to 0.0.0.0 instead of 127.0.0.1. This is required inside a Docker container
# because without it, Vite only listens on the container's loopback interface,
# which is unreachable from outside the container.
FROM deps AS dev
COPY . .
EXPOSE 3000
CMD ["npm", "run", "dev", "--", "--host"]

# ─── Stage 3: test ───────────────────────────────────────────────────────────
# Runs the Vitest unit test suite once ("vitest run") and exits.
# A non-zero exit code causes this Docker build stage to fail — useful in CI
# to prevent building a broken image.
# Note: Playwright E2E tests need a running backend + browser — they are not
# included here. Run them separately with docker-compose.test.yml + a browser.
FROM deps AS test
COPY . .
RUN npm run test

# ─── Stage 4: build ──────────────────────────────────────────────────────────
# Runs "tsc -b && vite build" to:
#   1. Type-check all TypeScript files and fail if there are type errors.
#   2. Bundle all JS/CSS/assets into the dist/ directory with content hashes.
FROM deps AS build
COPY . .
RUN npm run build

# ─── Stage 5: runtime ────────────────────────────────────────────────────────
# The final production image.
# Copies only the compiled dist/ output from the build stage.
# No Node.js, no source code, no devDependencies — just Nginx + static files.
# The nginx.conf file configures:
#   - SPA routing fallback (React Router support on page refresh)
#   - /api/* reverse proxy to the backend container
#   - Long-lived caching for hashed static assets
FROM nginx:stable-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
