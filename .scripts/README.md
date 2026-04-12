# `.scripts`

This folder holds a **checked-in copy** of `SuperMarketStockTakeAPI.dll` for scripts, CI, or teammates who need the plugin without building.

- **Update** this file when you cut a release (copy from `src/bin/Release/net472/SuperMarketStockTakeAPI.dll` after `dotnet build -c Release`).
- It is **not** ignored by `.gitignore`; commit it like any other asset.
