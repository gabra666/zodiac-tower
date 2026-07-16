# Zodiac Tower

A Blazor WebAssembly design and balance laboratory backed by a deterministic, Unity-compatible C# core.

## Projects

- `ZodiacTower.Core`: `netstandard2.1` and C# 9 domain library with floor rules, seeded generation, zodiac identity, and battle resolution.
- `ZodiacTower.Web`: standalone .NET 9 Blazor WebAssembly app with generator, duel, and balance analysis tools.
- `ZodiacTower.Core.Tests`: xUnit coverage for generation invariants and battle rules.

## Run

```powershell
dotnet restore
dotnet run --project ZodiacTower.Web
```

Open the URL printed by the development server. To run the tests:

```powershell
dotnet test
```

The generated `ZodiacTower.Core.dll` can later be copied into a Unity project's `Assets/Plugins` directory.

## GitHub Pages

Pushes to `main` automatically publish the Blazor app with the workflow in `.github/workflows/deploy-pages.yml`.

For the first deployment, open the repository's **Settings > Pages** and set **Source** to **GitHub Actions**. The published site is available at:

https://gabra666.github.io/zodiac-tower/
