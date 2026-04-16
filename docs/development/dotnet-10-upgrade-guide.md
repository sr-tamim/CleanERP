# CleanERP .NET 8 to .NET 10 Upgrade Guide

## Purpose

This document provides a practical, repo-specific guide for upgrading the full CleanERP solution from `.NET 8` to `.NET 10` using the `dotnet` CLI as the primary workflow.

The solution can be upgraded directly from `.NET 8` to `.NET 10`. You do not need to migrate to `.NET 9` first. However, because this skips one release, validation should include both `.NET 9` and `.NET 10` compatibility and breaking changes review.

## Recommended Approach

Use the `dotnet` CLI as the source of truth for:

- SDK selection
- package restore
- build validation
- test execution
- EF Core tooling
- package version inspection

Visual Studio can still be used for editing and debugging, but the upgrade itself should be validated from the CLI to avoid IDE-specific assumptions.

## Current Repository Baseline

At the time of writing, this repository contains:

- `1` solution file: `CleanERP.sln`
- `7` SDK-style projects targeting `net8.0`
- API containerization via Docker
- ASP.NET Core API project using `Microsoft.NET.Sdk.Web`
- EF Core and Npgsql packages in the persistence layer
- explicit `Microsoft.Extensions.*` package references in infrastructure and persistence

### Projects That Must Be Retargeted

- `src/Presentation/CleanERP.API/CleanERP.API.csproj`
- `src/Core/CleanERP.Application/CleanERP.Application.csproj`
- `src/Core/CleanERP.Domain/CleanERP.Domain.csproj`
- `src/Core/CleanERP.Shared/CleanERP.Shared.csproj`
- `src/Infrastructure/CleanERP.Infrastructure/CleanERP.Infrastructure.csproj`
- `src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj`
- `src/CompositionRoot/CleanERP.CompositionRoot/CleanERP.CompositionRoot.csproj`

### Container Files That Must Be Updated

- `src/Presentation/CleanERP.API/Dockerfile`
- `src/Presentation/CleanERP.API/Dockerfile.original`

## Before You Start

### 1. Install .NET 10 SDK

Verify the SDK installed on your machine:

```bash
dotnet --list-sdks
dotnet --version
```

You need a `.NET 10 SDK` installed locally before changing project targets to `net10.0`.

### 2. Confirm Supporting Toolchain

Check:

- Visual Studio or Rider version supports `.NET 10`
- CI agents have `.NET 10 SDK`
- Docker environment can pull `.NET 10` base images
- deployment targets are compatible with the new runtime

### 3. Create a Safety Branch

```bash
git checkout -b chore/upgrade-dotnet-10
git status --short
```

### 4. Capture a Pre-Upgrade Baseline

Before touching anything, confirm the current branch state:

```bash
dotnet restore CleanERP.sln
dotnet build CleanERP.sln
dotnet test CleanERP.sln
```

If the current `.NET 8` baseline is already failing, fix that first. Otherwise you will mix old failures with upgrade regressions.

## Upgrade Strategy

Use this order:

1. upgrade SDK/tooling
2. retarget all projects to `net10.0`
3. align NuGet package versions
4. update Docker runtime and SDK images
5. restore and build
6. run tests
7. validate EF Core tooling and runtime behavior
8. review breaking changes

This keeps framework-targeting changes separate from package changes and makes failures easier to isolate.

## Phase 1: Inspect the Solution with the CLI

Use these commands before editing:

```bash
dotnet sln CleanERP.sln list
dotnet list CleanERP.sln package
dotnet list CleanERP.sln package --outdated
```

Optional detailed inspection:

```bash
dotnet --info
dotnet nuget list source
```

## Phase 2: Retarget All Projects to .NET 10

In every `.csproj`, change:

```xml
<TargetFramework>net8.0</TargetFramework>
```

to:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### Affected Files

- `src/Presentation/CleanERP.API/CleanERP.API.csproj`
- `src/Core/CleanERP.Application/CleanERP.Application.csproj`
- `src/Core/CleanERP.Domain/CleanERP.Domain.csproj`
- `src/Core/CleanERP.Shared/CleanERP.Shared.csproj`
- `src/Infrastructure/CleanERP.Infrastructure/CleanERP.Infrastructure.csproj`
- `src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj`
- `src/CompositionRoot/CleanERP.CompositionRoot/CleanERP.CompositionRoot.csproj`

### Validation After Retargeting

Run:

```bash
dotnet restore CleanERP.sln
dotnet build CleanERP.sln
```

Do not move on if the target framework change alone causes obvious compile failures.

## Phase 3: Align Package Versions

Changing `TargetFramework` is not enough. Several package references in this repository should be aligned to the `.NET 10` generation.

### Packages That Should Move to 10.x

In general, keep these on matching major versions:

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Relational`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`

### Repo-Specific Notes

#### API Project

File:

- `src/Presentation/CleanERP.API/CleanERP.API.csproj`

Current notable packages:

- `Microsoft.AspNetCore.Authentication.JwtBearer` is still `8.x`
- `Swashbuckle.AspNetCore` is already on a later major than the target framework, so verify compatibility rather than assuming it must change

#### Persistence Project

File:

- `src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj`

Current state:

- EF Core packages are `9.x`
- `Npgsql.EntityFrameworkCore.PostgreSQL` is `9.x`
- `Microsoft.Extensions.*` abstractions are `9.x`

These should be aligned with the `.NET 10` move to avoid cross-generation mismatches.

#### Infrastructure Project

File:

- `src/Infrastructure/CleanERP.Infrastructure/CleanERP.Infrastructure.csproj`

Current state includes:

- `Microsoft.Extensions.*` packages at `9.x`
- legacy `Microsoft.AspNetCore.Http` and `Microsoft.AspNetCore.Http.Abstractions` package references at `2.3.0`
- `System.Security.Claims` package reference at `4.3.0`

### Important Cleanup Recommendation

Do not blindly upgrade these legacy references:

- `Microsoft.AspNetCore.Http`
- `Microsoft.AspNetCore.Http.Abstractions`
- `System.Security.Claims`

For modern ASP.NET Core projects, these are typically provided by the shared framework and should usually be removed instead of version-bumped.

This matters because carrying old compatibility packages during a framework upgrade often causes unnecessary dependency noise and can mask the real source of runtime or compile issues.

### CLI Commands to Review Package State

```bash
dotnet list CleanERP.sln package
dotnet list CleanERP.sln package --outdated
```

If you want to update a specific package from the CLI, the common pattern is:

```bash
dotnet add src/Presentation/CleanERP.API/CleanERP.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.*
```

Repeat for each explicitly referenced package that should move to the `10.x` train.

### Practical Recommendation

For this repository, do package alignment in two passes:

1. move the framework-coupled packages to `10.x`
2. remove obsolete explicit packages that are no longer needed on modern ASP.NET Core

That sequence reduces the chance of deleting a package and then losing track of whether a later build failure is caused by the removal or by the framework upgrade itself.

## Phase 4: Update Docker Images

This solution uses `.NET 8` container images today. Those must be updated along with the project target framework.

### Files

- `src/Presentation/CleanERP.API/Dockerfile`
- `src/Presentation/CleanERP.API/Dockerfile.original`

### Required Changes

Change:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```

to:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

### Docker Validation

After the project builds locally:

```bash
docker compose build
docker compose up
```

If your deployment pipeline uses cached base images, force a fresh pull during the first validation cycle.

## Phase 5: EF Core Tooling

If you use EF Core CLI commands, make sure the toolchain is aligned:

```bash
dotnet tool update --global dotnet-ef
dotnet ef --version
```

Then validate migrations and design-time services:

```bash
dotnet ef dbcontext info --project src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj --startup-project src/Presentation/CleanERP.API/CleanERP.API.csproj
```

If this fails, resolve that before generating or applying any new migrations.

## Phase 6: Build, Test, and Runtime Validation

### Core Validation Commands

```bash
dotnet clean CleanERP.sln
dotnet restore CleanERP.sln
dotnet build CleanERP.sln
dotnet test CleanERP.sln
```

If you want stricter output during upgrade verification:

```bash
dotnet build CleanERP.sln -warnaserror
```

### API Run Validation

```bash
dotnet run --project src/Presentation/CleanERP.API/CleanERP.API.csproj
```

Check at minimum:

- application starts cleanly
- Swagger loads
- authentication middleware still works
- database connection succeeds
- health endpoints still respond correctly

### Suggested Functional Checks

- login or token issuance path
- any endpoint using JWT authentication
- any endpoint hitting EF Core queries and writes
- startup registration from composition root
- middleware and exception handling behavior
- current-user resolution through `IHttpContextAccessor`

## Phase 7: Review Breaking Changes

A successful build is necessary, but not sufficient. Review breaking changes from both skipped and target releases:

- `.NET 9` compatibility and breaking changes
- `.NET 10` compatibility and breaking changes
- `ASP.NET Core 9` and `ASP.NET Core 10` breaking changes
- `EF Core 10` behavior changes

This is especially important for:

- authentication and authorization
- JSON serialization behavior
- OpenAPI and Swagger integration
- EF Core query translation and migrations
- hosting, middleware, and DI behavior

## Repo-Specific Risk Areas

These are the highest-value areas to verify in this solution.

### 1. Authentication Package Lag

The API project currently references:

- `Microsoft.AspNetCore.Authentication.JwtBearer` at `8.x`

If the target framework becomes `net10.0` while JWT remains on `8.x`, the project may still restore, but you increase the chance of runtime or API-surface mismatches. Upgrade it deliberately.

### 2. EF Core and Npgsql Are Already Ahead of the Framework

The persistence project currently runs `EF Core 9` packages on `net8.0`.

That is not inherently invalid, but it means the repository is already mixing framework and library generations. During the `.NET 10` upgrade, move EF Core and Npgsql together so you do not end up with another mixed state.

### 3. Legacy ASP.NET Core Package References

The infrastructure project includes:

- `Microsoft.AspNetCore.Http 2.3.0`
- `Microsoft.AspNetCore.Http.Abstractions 2.3.0`
- `System.Security.Claims 4.3.0`

These deserve cleanup during the upgrade. Old compatibility packages are a common source of confusion in modern SDK-style solutions.

### 4. Docker Image Version Drift

If project files target `net10.0` but the Dockerfile still uses `8.0` images, container builds will fail or run the wrong runtime. Update both together.

## Optional: Use `global.json`

This repository does not currently contain a `global.json`.

Adding one is recommended for team consistency:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

Adjust the exact SDK version to the one your team standardizes on.

Benefits:

- predictable local builds
- predictable CI builds
- reduced "works on my machine" variance

## Suggested Command Sequence

This is the recommended end-to-end CLI sequence for this repository.

```bash
git checkout -b chore/upgrade-dotnet-10

dotnet --list-sdks
dotnet --version

dotnet restore CleanERP.sln
dotnet build CleanERP.sln
dotnet test CleanERP.sln

dotnet sln CleanERP.sln list
dotnet list CleanERP.sln package
dotnet list CleanERP.sln package --outdated

# edit all TargetFramework values to net10.0
# align framework-coupled package versions to 10.x
# remove obsolete compatibility packages where appropriate
# update Dockerfiles from 8.0 images to 10.0

dotnet clean CleanERP.sln
dotnet restore CleanERP.sln
dotnet build CleanERP.sln
dotnet test CleanERP.sln

dotnet tool update --global dotnet-ef
dotnet ef --version
dotnet ef dbcontext info --project src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj --startup-project src/Presentation/CleanERP.API/CleanERP.API.csproj

dotnet run --project src/Presentation/CleanERP.API/CleanERP.API.csproj
docker compose build
docker compose up
```

## Troubleshooting

### `NETSDK1045` or unsupported target framework

Cause:

- `.NET 10 SDK` is not installed
- IDE or CI is using an older SDK

Fix:

- install `.NET 10 SDK`
- verify `dotnet --list-sdks`
- use `global.json` if multiple SDKs are installed

### Package downgrade or restore conflicts

Cause:

- mixed `8.x`, `9.x`, and `10.x` framework-coupled packages

Fix:

- align `Microsoft.*` and EF Core package majors
- remove obsolete explicit references
- run `dotnet list ... --outdated`

### Docker build fails after retargeting

Cause:

- Dockerfile still uses `.NET 8` images

Fix:

- change image tags from `8.0` to `10.0`
- rebuild without stale cache if necessary

### EF Core design-time commands fail

Cause:

- `dotnet-ef` tool mismatch
- startup project or provider version mismatch

Fix:

- update `dotnet-ef`
- verify provider and EF package versions match
- run `dotnet ef dbcontext info`

## Definition of Done

The upgrade is complete when all of the following are true:

- every project targets `net10.0`
- framework-coupled packages are aligned and restore cleanly
- obsolete compatibility packages are removed where appropriate
- Dockerfiles use `.NET 10` images
- `dotnet restore` succeeds
- `dotnet build` succeeds
- `dotnet test` succeeds
- `dotnet ef dbcontext info` succeeds if EF CLI is used
- the API starts successfully
- authentication, database, and health endpoints are verified
- CI passes using `.NET 10`

## Recommended Follow-Up Improvements

After the upgrade is stable, consider:

- adding `global.json`
- introducing `Directory.Build.props` for shared package version control
- using Central Package Management to reduce version drift
- documenting the required SDK version in onboarding docs

## Official References

- `.NET support policy`: https://dotnet.microsoft.com/en-us/platform/support/policy
- `.NET 10 compatibility`: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10
- `ASP.NET Core 9 breaking changes`: https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/9/overview?view=aspnetcore-10.0
- `ASP.NET Core 10 breaking changes`: https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/overview?view=aspnetcore-10.0
- `EF Core 10 what's new`: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew
