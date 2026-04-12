# SuperMarketStockTakeAPI

A [BepInEx](https://docs.bepinex.dev/) plugin for [**Supermarket Together**](https://store.steampowered.com/app/2709570/) that exposes in-game data over a small local HTTP API. Use it to build tools, overlays, or automations on top of the game.

## What it does

- Runs a **localhost-only** HTTP server (default **port 8080**).
- Serves **JSON** on documented `GET` routes; product-related payloads are refreshed about **once per second** on the Unity main thread and read from cache on the HTTP thread.
- Press **F8** in-game to write a placeholder stats file (`supermarket_stats.txt` next to the game folder) — intended for future expansion.

## Requirements

- **Supermarket Together** with **BepInEx** installed for that game.
- **.NET Framework 4.7.2** SDK (or compatible tooling) to build the project under [`src/MySupermarketDataMod.csproj`](src/MySupermarketDataMod.csproj).

## Build

From the `src` directory:

```bash
dotnet build -c Release
```

Output assembly: `MySupermarketDataMod.dll` (under `bin/Release/net472/` or similar).

## Install

Copy `MySupermarketDataMod.dll` into the game’s BepInEx plugin folder, for example:

`Supermarket Together/BepInEx/plugins/`

Restart the game. Check the BepInEx log for a line confirming the plugin loaded.

## Plugin identity (BepInEx)

| Field   | Value |
|--------|--------|
| **GUID** | `senpaihub.dev` |
| **Name** | SuperMarket StockTake Mod |
| **Version** | `0.0.1` (see [`src/PluginInfo.cs`](src/PluginInfo.cs)) |

## Local HTTP API

- **Base URL:** `http://localhost:8080/`
- **Methods:** **`GET` only** — other methods return **405**.
- **Unknown paths:** **404**
- **Content-Type:** `application/json; charset=utf-8`

### Endpoints

| Path | Description |
|------|-------------|
| `GET /ping` | Liveness check; body includes a version string (see [`PingApi`](src/Domain/Ping/PingApi.cs)). |
| `GET /stats` | Session snapshot: `time`, `frame`, `level` (current scene name). |
| `GET /products` | Shelf stock derived from `Data_Container` plus catalog info from `ProductListing` (camelCase JSON). If the session is not ready, the body may contain an `error` field. |
| `GET /spawnedProducts` | Cargo queue, shopping list, delivery boxes, and related counts from `ManagerBlackboard` (camelCase JSON). May return `error` if the game state is not ready. |

### Example

```bash
curl -s http://localhost:8080/ping
curl -s http://localhost:8080/stats
curl -s http://localhost:8080/products
curl -s http://localhost:8080/spawnedProducts
```

### Port and firewall

The listener binds to **localhost** only. If the server fails to start, check that nothing else is using port **8080**, and on some systems that **HTTP URL ACL** allows `http://localhost:8080/` for the game process.

## Repository layout

- [`src/`](src/) — C# plugin source, `.csproj`, and referenced game/BepInEx DLLs used for compilation.
- [`src/Plugin.cs`](src/Plugin.cs) — entry point; starts [`LocalHttpApiService`](src/Services/LocalHttpApiService.cs).
- [`.bruno/SupermarketSim/`](.bruno/SupermarketSim/) — [Bruno](https://www.usebruno.com/) API collection (ping, stats, products, spawned products) targeting `http://localhost:8080`.

## Bruno (API collection)

Open the repo in [Bruno](https://www.usebruno.com/) and import the collection under [`.bruno/SupermarketSim/`](.bruno/SupermarketSim/) (see `opencollection.yml`). With the game running and the mod loaded, requests hit the same localhost URLs as the `curl` examples above.

## Continuous integration

[`.github/workflows/dotnet-desktop.yml`](.github/workflows/dotnet-desktop.yml) runs `dotnet restore` and `dotnet build` on **Ubuntu** for pushes and pull requests to `develop`. The build **requires** `src/SuperMarketDll/` and `src/BepInExDll/` to be present in the checked-out tree (same as a local build). If those folders are not in the remote repository, add them or adjust the workflow (for example artifact download, self-hosted runner, or a conditional job).

## TODO

- [ ] Add filter parameters to `products` and `spawnedProducts` endpoints
- [ ] Rename `products` and `spawnedProducts` domain classes
- [ ] Clean up codebase to be more human-readable
- [ ] Flesh out OpenAPI specification (Bruno collection exists under `.bruno/`)
- [ ] Add release script and GitHub release action
- [ ] Update for easier internal versioning
- [ ] Cleanup 'dotnet build' warnings and remove un-used references
- [ ] Add response wrapper to all domain classes so errors are consistent for logging and HTTP status codes
- [ ] Improve how game and BepInEx DLLs are referenced (instead of checked-in `BepInExDll` and `SuperMarketDll` folders)

### Other ideas

- [ ] BepInEx config for listen port and product JSON refresh interval
- [ ] Document or publish JSON Schema (or OpenAPI components) for each response shape
- [ ] Optional: enforce `dotnet format --verify-no-changes` in CI after the codebase matches `.editorconfig` (line endings, explicit types, etc.)

## License / game assets

This project is a mod; **Supermarket Together** and its assets belong to their respective rights holders. Do not redistribute the game’s DLLs except as permitted by the game’s terms and applicable law.
