# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first to improve Docker layer caching
COPY ["src/P3K.GameServer.Api/P3K.GameServer.Api.csproj", "src/P3K.GameServer.Api/"]

# Restore dependencies
RUN dotnet restore "src/P3K.GameServer.Api/P3K.GameServer.Api.csproj"

# Copy the rest of the repository
COPY . .

# Publish application
RUN dotnet publish "src/P3K.GameServer.Api/P3K.GameServer.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render provides PORT environment variable.
# ASP.NET Core will listen on this port.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

EXPOSE 8080

ENTRYPOINT ["dotnet", "P3K.GameServer.Api.dll"]
