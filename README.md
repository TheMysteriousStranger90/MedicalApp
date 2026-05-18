# MedicalApp

[![CI](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/ci.yml/badge.svg)](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/ci.yml)
[![Docker Build](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/docker.yml/badge.svg)](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/docker.yml)
[![CodeQL](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/codeql.yml/badge.svg)](https://github.com/TheMysteriousStranger90/MedicalApp/actions/workflows/codeql.yml)

![Screenshot](Screenshots/Screen(1).png)

## Overview

A distributed healthcare appointment management system built with **.NET 9** and **gRPC**.  
Doctors and patients manage medical appointments through a secure Razor Pages web client that communicates with a backend gRPC service over mTLS.

## Features

### Doctor Schedule Management
- Create schedules with working hours and slot durations
- Set validity periods and manage availability
- View and manage individual time slots

### Appointment Management
- Book appointments based on a doctor's schedule
- View upcoming and past appointments
- Cancel or reschedule appointments
- Appointment status tracking

### Security
- JWT-based authentication via gRPC
- Cookie authentication in the web client (8-hour expiry, `SameSite=Strict`)
- Antiforgery (CSRF) protection
- Security response headers (`X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`)

## Screenshots

![](Screenshots/Screen(2).png)
![](Screenshots/Screen(3).png)
![](Screenshots/Screen(4).png)
![](Screenshots/Screen(5).png)
![](Screenshots/Screen(6).png)
![](Screenshots/Screen(7).png)
![](Screenshots/Screen(8).png)
![](Screenshots/Screen(9).png)
![](Screenshots/Screen(10).png)
![](Screenshots/Screen(11).png)
![](Screenshots/Screen(12).png)
![](Screenshots/Screen(13).png)
![](Screenshots/Screen(14).png)
![](Screenshots/Screen(15).png)
![](Screenshots/Screen(16).png)
![](Screenshots/Screen(17).png)
![](Screenshots/Screen(18).png)
![](Screenshots/Screen(19).png)
![](Screenshots/Screen(20).png)
![](Screenshots/Screen(21).png)
![](Screenshots/Screen(22).png)
![](Screenshots/Screen(23).png)

## Technical Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 / ASP.NET Core 9 |
| RPC | gRPC (`Grpc.AspNetCore` 2.70) |
| Web UI | Razor Pages |
| ORM | Entity Framework Core 9 |
| Database | SQL Server 2022 |
| Auth | ASP.NET Core Identity + JWT |
| Logging | Serilog (console + rolling file) |
| Observability | OpenTelemetry (traces + metrics) |
| Mapping | AutoMapper |
| Containers | Docker / Docker Compose |

## Architecture

```
┌──────────────────────────┐        gRPC/TLS        ┌──────────────────────────┐
│    Medical.Client        │ ──────────────────────► │   Medical.GrpcService    │
│  (Razor Pages, :7082)    │                         │  (gRPC server, :7084)    │
└──────────────────────────┘                         └──────────┬───────────────┘
                                                                │  EF Core
                                                     ┌──────────▼───────────────┐
                                                     │   SQL Server 2022        │
                                                     │     (:1433)              │
                                                     └──────────────────────────┘
```

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Rancher Desktop](https://rancherdesktop.io/)
- PowerShell 5.1+ (Windows) for certificate generation

### 1. Generate TLS Certificates

```powershell
# From the repository root — creates certs/server.pfx and certs/client.pfx
powershell -File makemedicalcerts.ps1
```

To avoid browser certificate warnings, trust the generated root CA on your machine:

```powershell
# Import the self-signed root CA into your Trusted Root store (run once)
$root = Get-ChildItem "cert:\CurrentUser\My" |
    Where-Object { $_.Subject -eq "CN=MedicalRootCA" } |
    Sort-Object NotBefore -Descending |
    Select-Object -First 1

$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root","CurrentUser")
$store.Open("ReadWrite")
$store.Add($root)
$store.Close()
Write-Host "Trusted: $($root.Thumbprint)"
```

### 2. Run with Docker Compose

```bash
docker compose up -d
```

Then open **https://localhost:7082** in your browser.

### 3. Run Locally (without Docker)

```bash
# Terminal 1 — gRPC backend
dotnet run --project src/Medical.GrpcService

# Terminal 2 — web client
dotnet run --project src/Medical.Client
```

Open **https://localhost:7082**.

### Default Credentials (seed data)

| Role | Email | Password |
|------|-------|----------|
| Doctor | doctor@example.com | `P@ssw0rd!` |
| Patient | patient@example.com | `P@ssw0rd!` |

## Health Checks

| Endpoint | Description |
|----------|-------------|
| `GET /health/live` | Liveness — process alive |
| `GET /health/ready` | Readiness — DB reachable |
| `GET /health` | Aggregate |

## Environment Variables (Production)

| Variable | Service | Description |
|----------|---------|-------------|
| `Kestrel__Certificates__Default__Path` | both | Path to `.pfx` file |
| `Kestrel__Certificates__Default__Password` | both | PFX password |
| `ConnectionStrings__DefaultConnection` | GrpcService | SQL Server connection string |
| `Token__Key` | GrpcService | JWT signing key (≥ 512 bits) |
| `Token__Issuer` | GrpcService | JWT issuer URL |
| `Token__Audience` | GrpcService | JWT audience URL |
| `GrpcClient__BaseAddress` | Client | gRPC service base URL |

## CI/CD

| Workflow | Trigger | Description |
|----------|---------|-------------|
| **CI** | push/PR to `develop`, `master` | Build + unit tests |
| **Docker Build** | push/PR to `develop`, `master` | Build both Docker images |
| **CodeQL** | push/PR + weekly schedule | Static security analysis |

## Project Structure

```
MedicalApp/
├── src/
│   ├── Medical.GrpcService/   # gRPC backend (EF Core, JWT, OTel)
│   └── Medical.Client/        # Razor Pages frontend
├── tests/
│   └── Medical.GrpcService.Tests/
├── certs/                     # Generated TLS certificates (git-ignored)
├── docker-compose.yml
├── makemedicalcerts.ps1       # Certificate generation script
├── Directory.Build.props      # Shared MSBuild properties
└── Directory.Packages.props   # Central NuGet package versions
```

## Contributing

Contributions are welcome. Please fork the repository and create a pull request targeting the `develop` branch.

## Author

Bohdan Harabadzhyu

## License

[MIT](LICENSE)

## YouTube Review

<details>
<summary>📺 Watch Video Review</summary>

[![YouTube](http://i.ytimg.com/vi/NF1lHzmazgs/hqdefault.jpg)](https://www.youtube.com/watch?v=NF1lHzmazgs)
</details>
