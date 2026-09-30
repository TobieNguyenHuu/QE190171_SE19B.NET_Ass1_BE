# Build & run the API on Render.com (Render has no native .NET runtime, so it deploys via Docker).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY *.sln ./
COPY TaskTrack.API/*.csproj TaskTrack.API/
COPY TaskTrack.Repo/*.csproj TaskTrack.Repo/
COPY TaskTrack.Service/*.csproj TaskTrack.Service/
RUN dotnet restore

COPY . .
RUN dotnet publish TaskTrack.API/TaskTrack.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Render injects PORT at runtime; Program.cs binds to it.
EXPOSE 10000
ENTRYPOINT ["dotnet", "TaskTrack.API.dll"]
