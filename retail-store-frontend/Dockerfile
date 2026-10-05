# RetailStore frontend multi-stage image.
# deps installs locked packages; dev serves Vite; test runs Vitest;
# build creates the production bundle; runtime serves it through Nginx.
# Keeping dependency installation separate lets source changes reuse npm ci.
FROM node:22-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

# Development server with HMR. Bind Vite to all container interfaces so the
# published port is reachable from outside the container.
FROM deps AS dev
COPY . .
EXPOSE 3000
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

# Run the frontend unit suite during CI image builds. A failing test stops build.
FROM deps AS test
COPY . .
RUN npm run test

# Type-check and bundle the production frontend.
FROM deps AS build
COPY . .
RUN npm run build

# Production image contains only Nginx and the compiled static assets.
FROM nginx:stable-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80