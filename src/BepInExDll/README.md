# BepInExDll (local only)

This folder is **not** in source control. Copy the **BepInEx core / dependency** assemblies that the project references (for example `BepInEx.dll`, Harmony, Mono.Cecil, MonoMod) from the **BepInEx** layout you use for Supermarket Together.

Typical source: your game folder’s `BepInEx/core/` (and any other paths that contain the DLL names listed in `MySupermarketDataMod.csproj` under `BepInExDll\…`).

Match the **BepInEx major version** you run in-game.

Do **not** commit these files; they are covered by `.gitignore`.
