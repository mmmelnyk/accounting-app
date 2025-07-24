# Use the official .NET 9 runtime image as base
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["AccountingCLI/AccountingCLI.csproj", "AccountingCLI/"]
COPY ["AccountingCLI.Tests/AccountingCLI.Tests.csproj", "AccountingCLI.Tests/"]

# Restore dependencies
RUN dotnet restore "AccountingCLI/AccountingCLI.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/AccountingCLI"
RUN dotnet build "AccountingCLI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AccountingCLI.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "AccountingCLI.dll"]
