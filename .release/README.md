# `.release`

`SuperMarketStockTakeAPI.dll` can be **committed** here so clones include a built plugin without compiling.

- **Local:** copy from `src/bin/Release/net472/SuperMarketStockTakeAPI.dll` after `dotnet build -c Release`, or use the [pre-push hook](../README.md#git-pre-push-hook-optional) (which copies the DLL here).
- **CI:** the workflow builds, copies into `.release/`, verifies, and uploads the same file as an **artifact**.
