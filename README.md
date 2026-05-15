# GameServer.Api

Simple fake game server Web API for student practice.

## What this project does

`GameServer.Api` simulates a live game server without a database. Data comes from configuration (`appsettings.json`) and selected values are randomized on each request.

## Endpoints

- `GET /api/server/status` - current fake server status
- `GET /api/players` - fake players list

Both `/api/*` endpoints require API key authentication.

## API key header

```http
X-API-KEY: dev-game-server-key
```

- Missing header -> `401 Unauthorized`
- Wrong key -> `403 Forbidden`

## Run the app

```bash
cd GameServer.Api
dotnet run
```

Open:

- OpenAPI JSON: `http://localhost:5026/openapi/v1.json`
- Scalar UI: `http://localhost:5026/scalar/v1`

## Example curl requests

```bash
curl -H "X-API-KEY: dev-game-server-key" http://localhost:5026/api/server/status
```

```bash
curl -H "X-API-KEY: dev-game-server-key" http://localhost:5026/api/players
```

```bash
curl http://localhost:5026/scalar/v1
```
