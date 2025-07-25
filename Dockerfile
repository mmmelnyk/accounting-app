# Use the official .NET 9 runtime image as base
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["AccountingApp/AccountingApp.csproj", "AccountingApp/"]
COPY ["AccountingApp.Tests/AccountingApp.Tests.csproj", "AccountingApp.Tests/"]

# Restore dependencies
RUN dotnet restore "AccountingApp/AccountingApp.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/AccountingApp"
RUN dotnet build "AccountingApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AccountingApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime image
FROM base AS final
WORKDIR /app

# Create data directory for JSON file persistence
RUN mkdir -p /app/data

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables for better console experience
ENV DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=true
ENV TERM=xterm

# Entry point
ENTRYPOINT ["dotnet", "AccountingApp.dll"]
