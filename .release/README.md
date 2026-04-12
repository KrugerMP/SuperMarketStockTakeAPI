# `.release` (generated output)

`SuperMarketStockTakeAPI.dll` is **not committed**. It is produced by:

- **Local:** `dotnet build` and the optional [`pre-push` hook](../README.md#git-pre-push-hook-optional) (which copies the built DLL here).
- **CI:** the GitHub Actions workflow copies the Release build output here, verifies the file, and uploads it as a workflow **artifact**.
