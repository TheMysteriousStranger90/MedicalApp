# MedicalApp
![Image 1](Screenshots/Screen(1).png)

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

![Image 1](Screenshots/Screen(2).png)
![Image 1](Screenshots/Screen(3).png)
![Image 1](Screenshots/Screen(4).png)
![Image 1](Screenshots/Screen(5).png)
![Image 1](Screenshots/Screen(6).png)
![Image 1](Screenshots/Screen(7).png)
![Image 1](Screenshots/Screen(8).png)
![Image 1](Screenshots/Screen(9).png)
![Image 1](Screenshots/Screen(10).png)
![Image 1](Screenshots/Screen(11).png)
![Image 1](Screenshots/Screen(12).png)
![Image 1](Screenshots/Screen(13).png)
![Image 1](Screenshots/Screen(14).png)
![Image 1](Screenshots/Screen(15).png)
![Image 1](Screenshots/Screen(16).png)
![Image 1](Screenshots/Screen(17).png)
![Image 1](Screenshots/Screen(18).png)
![Image 1](Screenshots/Screen(19).png)
![Image 1](Screenshots/Screen(20).png)
![Image 1](Screenshots/Screen(21).png)
![Image 1](Screenshots/Screen(22).png)
![Image 1](Screenshots/Screen(23).png)

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