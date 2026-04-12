# SuperMarketDll (local only)

This folder is **not** in source control. Copy **managed assemblies** from your **Supermarket Together** installation so the paths in `SuperMarketStockTakeAPI.csproj` resolve (for example `Assembly-CSharp.dll`, `UnityEngine*.dll`, and the other referenced DLL names).

Typical locations:

- **Windows (Steam):** `…/Steam/steamapps/common/Supermarket Together/<GameName>_Data/Managed/`
- **Linux / Steam Play:** under your Steam library’s `steamapps/common/Supermarket Together/`, same `…_Data/Managed/` pattern.

Copy the DLLs into this directory (same filenames as in the `.csproj` `SuperMarketDll\…` references). Use files from an install that **matches the game version** you mod against.

Do **not** commit these files; they are covered by `.gitignore`.
