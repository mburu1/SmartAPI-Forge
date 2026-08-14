# SmartAPI Forge — Dashboard

A minimal Angular status dashboard for the SmartAPI Forge API: a KPI row of
stat tiles (health, environment, database provider, uptime, version, server
time) that polls `GET /system/status` on the running Api every 10 seconds.

## Run it

Start the Api first (from the repo root):

```bash
dotnet run --project src/SmartAPIForge.Api
```

Then, from this directory:

```bash
npm install
ng serve
```

Open `http://localhost:4200`. The Api's CORS policy already allows this
origin (see `Cors:AllowedOrigins` in `appsettings.json`).

If your Api runs on a different host/port, update `API_BASE_URL` in
`src/app/api-config.ts`.

## Test

```bash
ng test
```

## Build

```bash
ng build
```

Output goes to `dist/`.

## Roadmap

This is intentionally scoped to what the Api currently exposes. Real
endpoint-level observability (request rates, latency, error budgets) is
listed in the root [README's roadmap](../../README.md#roadmap) — it needs
the Api to emit that telemetry first.
