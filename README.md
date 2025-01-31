# MedicalApp

## Overview
A distributed healthcare appointment management system built with .NET 8 and gRPC, enabling doctors and patients to manage medical appointments efficiently.

## Features
- **Doctor Schedule Management**
    - Create schedules with working hours
    - Set validity periods for schedules
    - Define appointment slot duration
    - Manage schedule availability
    - View and manage time slots
- **Appointment Management**
    - Book appointments based on doctor's schedule
    - View upcoming and past appointments
    - Cancel or reschedule appointments
    - Appointment status tracking

## Technical Stack
- .NET 8.0
- ASP.NET Core
- gRPC
- Entity Framework Core
- SQL Server
- Razor Pages
- AutoMapper
- Serilog

## Architecture
- **Medical.GrpcService**: Backend gRPC service
- **Medical.Client**: Web client application

## SSL Certificate Setup
1. Run makemedicalcerts.ps1.ps1 script
2. Copy server.pfx to Medical.GrpcService root
3. Copy client.pfx to Medical.Client root
4. Set build action to "Content" and "Copy if newer" for both certificates
5. Run PowerShell as administrator and go to the Medical.Client directory and install client certificate
6. Run the following command:
```powershell
Import-PfxCertificate -FilePath .\client.pfx -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString -String "P@ssw0rd!" -Force -AsPlainText)
```

## Contributing

Contributions are welcome. Please fork the repository and create a pull request with your changes.

## Author

Bohdan Harabadzhyu

## License

[MIT](https://choosealicense.com/licenses/mit/)