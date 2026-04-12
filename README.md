# SuperMarketStockTakeAPI

A [BepInEx](https://docs.bepinex.dev/) plugin for [**Supermarket Together**](https://store.steampowered.com/app/2709570/) that exposes in-game data over a small local HTTP API. Use it to build tools, overlays, or automations on top of the game.

## What it does

- Runs a **localhost-only** HTTP server (default **port 8080**).
- Serves **JSON** on documented `GET` routes; product-related payloads are refreshed about **once per second** on the Unity main thread and read from cache on the HTTP thread.
- Press **F8** in-game to write a placeholder stats file (`supermarket_stats.txt` next to the game folder) — intended for future expansion.

## Requirements

- **Supermarket Together** with **BepInEx** installed for that game.
- **.NET Framework 4.7.2** SDK (or compatible tooling) to build the project under [`src/SuperMarketStockTakeAPI.csproj`](src/SuperMarketStockTakeAPI.csproj).

## Developing locally

This repository **does not** ship the game’s managed assemblies or BepInEx DLLs (see [License / game assets](#license--game-assets)). To compile, you must copy them from **your own install** into the folders the project references:

| Folder | What to put there |
|--------|-------------------|
| [`src/SuperMarketDll/`](src/SuperMarketDll/) | Unity / game **Managed** DLLs (`Assembly-CSharp.dll`, `UnityEngine*.dll`, etc.) — every file name referenced under `SuperMarketDll\` in [`SuperMarketStockTakeAPI.csproj`](src/SuperMarketStockTakeAPI.csproj). |
| [`src/BepInExDll/`](src/BepInExDll/) | **BepInEx** core libraries (`BepInEx.dll`, Harmony, Mono.Cecil, …) matching the `BepInExDll\` references in the same `.csproj`. |

**Game DLLs:** From your Steam install, open the game’s `…_Data/Managed/` directory (Unity layout) and copy the needed assemblies into `src/SuperMarketDll/`. Use a game **patch version** consistent with what you test against.

**BepInEx DLLs:** From the same game folder, use the BepInEx layout you already run (commonly files under `BepInEx/core/`) and copy the referenced DLLs into `src/BepInExDll/`.

After both folders contain the expected files, build from the repository root:

```bash
dotnet build src/SuperMarketStockTakeAPI.csproj -c Release
```

Output: `src/bin/Release/net472/SuperMarketStockTakeAPI.dll` (paths relative to the repo root).

### Git `pre-push` hook (optional)

To run **`dotnet build`** automatically before every **`git push`** (and block the push if the build fails), point Git at the tracked hooks directory once per clone:

```bash
git config core.hooksPath .githooks
```

The script is [`.githooks/pre-push`](.githooks/pre-push). After a successful build it copies **`SuperMarketStockTakeAPI.dll`** into **`.release/`** (same layout as CI). To push without building (e.g. emergency): **`git push --no-verify`**.

## Install

Copy `SuperMarketStockTakeAPI.dll` into the game’s BepInEx plugin folder, for example:

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

- [`src/`](src/) — C# plugin source and `.csproj`. Game and BepInEx assemblies live under [`src/SuperMarketDll/`](src/SuperMarketDll/) and [`src/BepInExDll/`](src/BepInExDll/) **on your machine only** (see [Developing locally](#developing-locally)); tracked files there are setup notes, not binaries.
- [`src/Plugin.cs`](src/Plugin.cs) — entry point; starts [`LocalHttpApiService`](src/Services/LocalHttpApiService.cs).
- [`.bruno/SupermarketSim/`](.bruno/SupermarketSim/) — [Bruno](https://www.usebruno.com/) API collection (ping, stats, products, spawned products) targeting `http://localhost:8080`.
- [`.release/`](.release/) — **staging copy** of the built `SuperMarketStockTakeAPI.dll` (you can commit the DLL; see [`.release/README.md`](.release/README.md)); produced locally by the [pre-push hook](#git-pre-push-hook-optional) and in CI before artifact upload.
- [`.scripts/`](.scripts/) — **tracked** copy of `SuperMarketStockTakeAPI.dll` for scripts or consumers who clone without building (see [`.scripts/README.md`](.scripts/README.md)); update when you release.

## Bruno (API collection)

Open the repo in [Bruno](https://www.usebruno.com/) and import the collection under [`.bruno/SupermarketSim/`](.bruno/SupermarketSim/) (see `opencollection.yml`). With the game running and the mod loaded, requests hit the same localhost URLs as the `curl` examples above.

## Continuous integration

[`.github/workflows/dotnet-desktop.yml`](.github/workflows/dotnet-desktop.yml) runs on pushes and pull requests to `develop` (and **workflow_dispatch**):

1. Verifies the project and setup readmes exist.
2. **`dotnet restore`** / **`dotnet build`** (Release).
3. Copies **`src/bin/Release/net472/SuperMarketStockTakeAPI.dll`** to **[`.release/`](.release/)** and checks that **`.release/SuperMarketStockTakeAPI.dll`** exists.
4. Uploads that DLL as a workflow **artifact** named **`SuperMarketStockTakeAPI`** (download from the Actions run).

**Hosted runners:** The build step **requires** `src/SuperMarketDll/` and `src/BepInExDll/` to contain the referenced assemblies (same as a local build). This public repo does not commit those files, so **GitHub-hosted `ubuntu-latest` will fail at `dotnet build`** unless you add a bootstrap step (cache, artifact, or install from an allowed source). Use a **self-hosted runner** with DLLs already present, or a private pipeline, if you need green CI without vendoring game binaries.

The **[`.release/README.md`](.release/README.md)** explains the folder. Commit **`.release/SuperMarketStockTakeAPI.dll`** when you want the built mod in the repo (same idea as [`.scripts/`](.scripts/)).


## TODO

- [ ] Add filter parameters to `products` and `spawnedProducts` endpoints
- [ ] Rename `products` and `spawnedProducts` domain classes
- [ ] Clean up codebase to be more human-readable
- [ ] Flesh out OpenAPI specification (Bruno collection exists under `.bruno/`)
- [ ] Add release script and GitHub release action
- [ ] Update for easier internal versioning
- [ ] Cleanup 'dotnet build' warnings and remove un-used references
- [ ] Add response wrapper to all domain classes so errors are consistent for logging and HTTP status codes
- [ ] Improve how game and BepInEx DLLs are referenced (NuGet / slimmer refs; game assemblies remain local-only for a public repo)

### Other ideas

- [ ] BepInEx config for listen port and product JSON refresh interval
- [ ] Document or publish JSON Schema (or OpenAPI components) for each response shape
- [ ] Optional: enforce `dotnet format --verify-no-changes` in CI after the codebase matches `.editorconfig` (line endings, explicit types, etc.)

## License / game assets

This project is a mod; **Supermarket Together** and its assets belong to their respective rights holders. Do not redistribute the game’s DLLs except as permitted by the game’s terms and applicable law.
