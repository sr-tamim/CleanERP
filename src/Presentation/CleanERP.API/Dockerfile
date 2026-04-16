# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

COPY ["src/Presentation/CleanERP.API/CleanERP.API.csproj", "src/Presentation/CleanERP.API/"]
COPY ["src/Core/CleanERP.Application/CleanERP.Application.csproj", "src/Core/CleanERP.Application/"]
COPY ["src/Core/CleanERP.Domain/CleanERP.Domain.csproj", "src/Core/CleanERP.Domain/"]
COPY ["src/Core/CleanERP.Shared/CleanERP.Shared.csproj", "src/Core/CleanERP.Shared/"]
COPY ["src/CompositionRoot/CleanERP.CompositionRoot/CleanERP.CompositionRoot.csproj", "src/CompositionRoot/CleanERP.CompositionRoot/"]
COPY ["src/Infrastructure/CleanERP.Infrastructure/CleanERP.Infrastructure.csproj", "src/Infrastructure/CleanERP.Infrastructure/"]
COPY ["src/Infrastructure/CleanERP.Persistence/CleanERP.Persistence.csproj", "src/Infrastructure/CleanERP.Persistence/"]
RUN dotnet restore "./src/Presentation/CleanERP.API/CleanERP.API.csproj"

COPY . .
WORKDIR "/repo/src/Presentation/CleanERP.API"
RUN dotnet build "./CleanERP.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanERP.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanERP.API.dll"]
