using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.SeedData;

public static class SeedDataInitializer
{
    public static async Task SeedUsersAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        var roles = new List<Role>
        {
            new Role { Name = "Doctor" },
            new Role { Name = "Patient" },
            new Role { Name = "Administrator" }
        };

        foreach (var role in roles)
        {
            if (!(await roleManager.RoleExistsAsync(role.Name ?? throw new InvalidOperationException())))
            {
                await roleManager.CreateAsync(role);
            }
        }

        // Seed Admin
        var adminEmail = "admin@example.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }

        // Seed Doctors
        var doctors = new List<Doctor>
        {
            new Doctor
            {
                UserName = "doctor1@example.com",
                Email = "doctor1@example.com",
                EmailConfirmed = true,
                FullName = "Dr. John Smith",
                Specialization = "Cardiology",
                LicenseNumber = "MD12345",
                Education = "Harvard Medical School",
                Experience = "15 years",
                ConsultationFee = 150.00m
            },
            new Doctor
            {
                UserName = "doctor2@example.com",
                Email = "doctor2@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Sarah Johnson",
                Specialization = "Pediatrics",
                LicenseNumber = "MD67890",
                Education = "Yale School of Medicine",
                Experience = "10 years",
                ConsultationFee = 120.00m
            }
        };

        foreach (var doctor in doctors)
        {
            if (await userManager.FindByEmailAsync(doctor.Email) == null)
            {
                var result = await userManager.CreateAsync(doctor, "Doctor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctor, "Doctor");
                }
            }
        }

        // Seed Patients
        var patients = new List<Patient>
        {
            new Patient
            {
                UserName = "patient1@example.com",
                Email = "patient1@example.com",
                EmailConfirmed = true,
                FullName = "Alice Brown",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = Gender.Female,
                Phone = "123-456-7890",
                Address = "123 Main St",
                BloodGroup = "A+",
                EmergencyContact = "Bob Brown: 123-555-0123"
            },
            new Patient
            {
                UserName = "patient2@example.com",
                Email = "patient2@example.com",
                EmailConfirmed = true,
                FullName = "Mike Wilson",
                DateOfBirth = new DateTime(1985, 8, 22),
                Gender = Gender.Male,
                Phone = "234-567-8901",
                Address = "456 Oak Ave",
                BloodGroup = "O+",
                EmergencyContact = "Jane Wilson: 234-555-0123"
            }
        };

        foreach (var patient in patients)
        {
            if (await userManager.FindByEmailAsync(patient.Email) == null)
            {
                var result = await userManager.CreateAsync(patient, "Patient123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patient, "Patient");
                }
            }
        }
    }

    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (!context.Schedules.Any())
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync();
            if (doctor != null)
            {
                var schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        DoctorId = doctor.Id,
                        StartTime = DateTime.Today.AddHours(9),
                        EndTime = DateTime.Today.AddHours(17),
                        DayOfWeek = DayOfWeek.Monday,
                        IsAvailable = true,
                        MaxAppointments = 8,
                        IsRecurring = true
                    },
                    new Schedule
                    {
                        DoctorId = doctor.Id,
                        StartTime = DateTime.Today.AddHours(9),
                        EndTime = DateTime.Today.AddHours(17),
                        DayOfWeek = DayOfWeek.Wednesday,
                        IsAvailable = true,
                        MaxAppointments = 8,
                        IsRecurring = true
                    }
                };

                await context.Schedules.AddRangeAsync(schedules);
                await context.SaveChangesAsync();
            }
        }

        if (!context.Appointments.Any())
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync();
            var patient = await context.Patients.FirstOrDefaultAsync();

            if (doctor != null && patient != null)
            {
                var appointment = new Appointment
                {
                    DoctorId = doctor.Id,
                    PatientId = patient.Id,
                    AppointmentDate = DateTime.Today.AddDays(1).AddHours(10),
                    Status = AppointmentStatus.Scheduled,
                    Fee = (double)doctor.ConsultationFee,
                    Symptoms = "Regular checkup",
                    Notes = "First visit"
                };

                await context.Appointments.AddAsync(appointment);
                await context.SaveChangesAsync();
            }
        }
    }
}