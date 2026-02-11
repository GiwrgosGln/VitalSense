FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Install wget for healthchecks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["VitalSense.sln", "./"]
COPY ["VitalSense.Api/*.csproj", "./VitalSense.Api/"]
COPY ["VitalSense.Application/*.csproj", "./VitalSense.Application/"]
COPY ["VitalSense.Domain/*.csproj", "./VitalSense.Domain/"]
COPY ["VitalSense.Infrastructure/*.csproj", "./VitalSense.Infrastructure/"]
COPY ["VitalSense.Shared/*.csproj", "./VitalSense.Shared/"]

# Restore dependencies
RUN dotnet restore "VitalSense.Api/VitalSense.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish "VitalSense.Api/VitalSense.Api.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app

# Create directory for DataProtection keys
RUN mkdir -p /keys && chmod 700 /keys

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VitalSense.Api.dll"]