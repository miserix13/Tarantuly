# Tarantuly

A framework for generating CRUD Blazor components in an ASP.NET Core web app using EntityFrameworkCore.

## Current bootstrap

This repository now contains an initial three-project scaffold:

- `Tarantuly` (core library)
  - CRUD entity contracts and options attributes
  - EF Core metadata discovery (`EfCrudModelDiscovery`)
  - generation pipeline abstractions (`ICrudTemplate`, `CrudGenerationEngine`)
  - first MudBlazor list template (`MudBlazorListTemplate`)
- `src/Tarantuly.Generator` (Roslyn source generator skeleton)
  - emits a generated marker class to validate analyzer wiring
- `src/Tarantuly.Cli` (CLI scaffold)
  - provides `scaffold` command for real `DbContext` discovery from a project assembly
  - writes generated files to disk

## Quick start

Build everything:

```bash
dotnet build Tarantuly.slnx
```

Run the CLI scaffold command (project path + context type):

```bash
dotnet run --project src/Tarantuly.Cli -- scaffold --project ./MyApp/MyApp.csproj --context AppDbContext --output Generated --namespace Tarantuly.Generated
```

Alternatively, provide a prebuilt assembly directly:

```bash
dotnet run --project src/Tarantuly.Cli -- scaffold --assembly ./MyApp/bin/Debug/net10.0/MyApp.dll --context AppDbContext --output Generated --namespace Tarantuly.Generated
```

Current generated templates:

- `*List.razor`
- `*Details.razor`
- `*Upsert.razor`

Current context creation support:

- parameterless `DbContext` constructor
- design-time factory with `CreateDbContext(string[])`

## End-to-end sample in this repo

This solution includes `src/Tarantuly.SampleApp` with:

- `Data/AppDbContext.cs`
- `Data/Product.cs`

Run generation against the sample app:

```bash
dotnet run --project src/Tarantuly.Cli -- scaffold --project src/Tarantuly.SampleApp/Tarantuly.SampleApp.csproj --context AppDbContext --output GeneratedSample --namespace Tarantuly.Generated
```

Expected output files:

- `GeneratedSample/Product/ProductList.razor`
- `GeneratedSample/Product/ProductDetails.razor`
- `GeneratedSample/Product/ProductUpsert.razor`
