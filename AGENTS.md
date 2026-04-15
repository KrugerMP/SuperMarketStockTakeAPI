# AGENTS.md

## Cursor Cloud specific instructions

### Project overview

This is a **BepInEx plugin** for the Steam game **Supermarket Together**. It compiles to a single .NET Framework 4.7.2 DLL (`SuperMarketStockTakeAPI.dll`) that injects a localhost-only HTTP JSON API (port 8080) into the game process.

### Build toolchain

- **.NET SDK 8.x** is required. The project targets `net472` using `Microsoft.NETFramework.ReferenceAssemblies` for cross-platform builds.
- Build: `dotnet build src/SuperMarketStockTakeAPI.csproj -c Release`
- Restore: `dotnet restore src/SuperMarketStockTakeAPI.csproj`

### Proprietary game DLLs (build blocker)

`dotnet build` **will fail** in this cloud environment because the project references ~90 proprietary DLLs from the Supermarket Together game and BepInEx framework. These live in `src/SuperMarketDll/` and `src/BepInExDll/` which are `.gitignore`-d. There is **no way** to obtain them without a personal Steam game installation. This is a known, expected limitation — not a setup bug. See `README.md` "Developing locally" section.

Pre-built DLL artifacts are committed at:
- `.release/SuperMarketStockTakeAPI.dll`
- `.scripts/SuperMarketStockTakeAPI.dll`

### Linting

- **`dotnet format src/SuperMarketStockTakeAPI.csproj --verify-no-changes`** runs code-style analysis using the extensive `.editorconfig` rules. This works without the game DLLs.
- The `.editorconfig` enforces strict C# naming conventions (PascalCase types/properties, camelCase locals, `_camelCase` private readonly fields, `Async` suffix for async methods, `var` is banned).

### Testing

There are **no automated tests** (unit or integration) in this codebase. The plugin can only be tested by loading it into the actual Supermarket Together game with BepInEx installed, then hitting the HTTP API (e.g. `curl http://localhost:8080/ping`). A [Bruno](https://www.usebruno.com/) API collection is provided in `.bruno/SupermarketSim/`.

### What agents can verify without the game

1. `dotnet restore` — NuGet package resolution
2. `dotnet format --verify-no-changes` — code style compliance
3. The pre-built DLL exists at `.release/SuperMarketStockTakeAPI.dll`
