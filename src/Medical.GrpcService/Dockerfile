# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /repo

# Restore dependencies (layer cache friendly)
COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/Medical.GrpcService/Medical.GrpcService.csproj src/Medical.GrpcService/
RUN dotnet restore src/Medical.GrpcService/Medical.GrpcService.csproj

# Copy source & publish
COPY src/Medical.GrpcService/ src/Medical.GrpcService/
RUN dotnet publish src/Medical.GrpcService/Medical.GrpcService.csproj \
        -c Release \
        -o /app/publish \
        --no-restore

# ── Runtime image ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system --gid 1001 appgroup \
 && adduser  --system --uid 1001 --ingroup appgroup appuser
USER appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

# gRPC over HTTPS (TLS terminated in the container)
EXPOSE 7084
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:7084

ENTRYPOINT ["dotnet", "Medical.GrpcService.dll"]