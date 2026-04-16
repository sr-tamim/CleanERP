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

COPY ["src/Presentation/GoldenFiberERP.API/GoldenFiberERP.API.csproj", "src/Presentation/GoldenFiberERP.API/"]
COPY ["src/Core/GoldenFiberERP.Application/GoldenFiberERP.Application.csproj", "src/Core/GoldenFiberERP.Application/"]
COPY ["src/Core/GoldenFiberERP.Domain/GoldenFiberERP.Domain.csproj", "src/Core/GoldenFiberERP.Domain/"]
COPY ["src/Core/GoldenFiberERP.Shared/GoldenFiberERP.Shared.csproj", "src/Core/GoldenFiberERP.Shared/"]
COPY ["src/CompositionRoot/GoldenFiberERP.CompositionRoot/GoldenFiberERP.CompositionRoot.csproj", "src/CompositionRoot/GoldenFiberERP.CompositionRoot/"]
COPY ["src/Infrastructure/GoldenFiberERP.Infrastructure/GoldenFiberERP.Infrastructure.csproj", "src/Infrastructure/GoldenFiberERP.Infrastructure/"]
COPY ["src/Infrastructure/GoldenFiberERP.Persistence/GoldenFiberERP.Persistence.csproj", "src/Infrastructure/GoldenFiberERP.Persistence/"]
RUN dotnet restore "./src/Presentation/GoldenFiberERP.API/GoldenFiberERP.API.csproj"

COPY . .
WORKDIR "/repo/src/Presentation/GoldenFiberERP.API"
RUN dotnet build "./GoldenFiberERP.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GoldenFiberERP.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoldenFiberERP.API.dll"]
