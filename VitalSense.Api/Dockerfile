FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ../VitalSense.sln ./
COPY VitalSense.Api/*.csproj ./VitalSense.Api/
COPY VitalSense.Application/*.csproj ./VitalSense.Application/
COPY VitalSense.Domain/*.csproj ./VitalSense.Domain/
COPY VitalSense.Infrastructure/*.csproj ./VitalSense.Infrastructure/
COPY VitalSense.Shared/*.csproj ./VitalSense.Shared/

# Restore dependencies
RUN dotnet restore VitalSense.Api/VitalSense.Api.csproj

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish VitalSense.Api/VitalSense.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directory for DataProtection keys
RUN mkdir -p /keys && chmod 700 /keys

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "VitalSense.Api.dll"]