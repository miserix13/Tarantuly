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
- `src/Tarantuly.Cli` (CLI scaffold skeleton)
	- provides `scaffold` command and writes generated files to disk

## Quick start

Build everything:

```bash
dotnet build Tarantuly.slnx
```

Run the CLI scaffold command:

```bash
dotnet run --project src/Tarantuly.Cli -- scaffold --output Generated --namespace Tarantuly.Generated
```

The current CLI generates a sample MudBlazor list component from an in-memory demo model. The next iteration will replace demo model input with real `DbContext` discovery and multi-template CRUD output (List, Details, Create, Edit).