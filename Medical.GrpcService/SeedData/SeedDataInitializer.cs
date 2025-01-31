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
            },
            new Doctor
            {
                UserName = "doctor3@example.com",
                Email = "doctor3@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Michael Brown",
                Specialization = "Orthopedics",
                LicenseNumber = "MD54321",
                Education = "Stanford University School of Medicine",
                Experience = "12 years",
                ConsultationFee = 130.00m
            },
            new Doctor
            {
                UserName = "doctor4@example.com",
                Email = "doctor4@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Emily Davis",
                Specialization = "Dermatology",
                LicenseNumber = "MD98765",
                Education = "Johns Hopkins School of Medicine",
                Experience = "8 years",
                ConsultationFee = 110.00m
            },
            new Doctor
            {
                UserName = "doctor5@example.com",
                Email = "doctor5@example.com",
                EmailConfirmed = true,
                FullName = "Dr. James Wilson",
                Specialization = "Neurology",
                LicenseNumber = "MD11223",
                Education = "Mayo Clinic Alix School of Medicine",
                Experience = "20 years",
                ConsultationFee = 200.00m
            },
            new Doctor
            {
                UserName = "doctor6@example.com",
                Email = "doctor6@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Linda Martinez",
                Specialization = "Oncology",
                LicenseNumber = "MD33445",
                Education = "University of California, San Francisco",
                Experience = "18 years",
                ConsultationFee = 180.00m
            },
            new Doctor
            {
                UserName = "doctor7@example.com",
                Email = "doctor7@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Robert Taylor",
                Specialization = "Psychiatry",
                LicenseNumber = "MD55667",
                Education = "Columbia University Vagelos College of Physicians and Surgeons",
                Experience = "14 years",
                ConsultationFee = 160.00m
            },
            new Doctor
            {
                UserName = "doctor8@example.com",
                Email = "doctor8@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Patricia Anderson",
                Specialization = "Endocrinology",
                LicenseNumber = "MD77889",
                Education = "University of Pennsylvania Perelman School of Medicine",
                Experience = "16 years",
                ConsultationFee = 170.00m
            },
            new Doctor
            {
                UserName = "doctor9@example.com",
                Email = "doctor9@example.com",
                EmailConfirmed = true,
                FullName = "Dr. William Thomas",
                Specialization = "Gastroenterology",
                LicenseNumber = "MD99001",
                Education = "Washington University in St. Louis School of Medicine",
                Experience = "13 years",
                ConsultationFee = 140.00m
            },
            new Doctor
            {
                UserName = "doctor10@example.com",
                Email = "doctor10@example.com",
                EmailConfirmed = true,
                FullName = "Dr. Elizabeth Jackson",
                Specialization = "Rheumatology",
                LicenseNumber = "MD22334",
                Education = "University of Chicago Pritzker School of Medicine",
                Experience = "11 years",
                ConsultationFee = 125.00m
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
            },
            new Patient
            {
                UserName = "patient3@example.com",
                Email = "patient3@example.com",
                EmailConfirmed = true,
                FullName = "Sarah Johnson",
                DateOfBirth = new DateTime(1992, 3, 10),
                Gender = Gender.Female,
                Phone = "345-678-9012",
                Address = "789 Pine St",
                BloodGroup = "B-",
                EmergencyContact = "John Johnson: 345-555-0123"
            },
            new Patient
            {
                UserName = "patient4@example.com",
                Email = "patient4@example.com",
                EmailConfirmed = true,
                FullName = "David Martinez",
                DateOfBirth = new DateTime(1988, 11, 30),
                Gender = Gender.Male,
                Phone = "456-789-0123",
                Address = "321 Elm St",
                BloodGroup = "AB+",
                EmergencyContact = "Maria Martinez: 456-555-0123"
            },
            new Patient
            {
                UserName = "patient5@example.com",
                Email = "patient5@example.com",
                EmailConfirmed = true,
                FullName = "Emily Chen",
                DateOfBirth = new DateTime(1995, 7, 25),
                Gender = Gender.Female,
                Phone = "567-890-1234",
                Address = "654 Maple Ave",
                BloodGroup = "O-",
                EmergencyContact = "William Chen: 567-555-0123"
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
                        StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                        EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                        DayOfWeek = DayOfWeek.Monday,
                        IsAvailable = true,
                        SlotDurationMinutes = 30,
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddMonths(3),
                        Notes = "Regular Monday schedule"
                    },
                    new Schedule
                    {
                        DoctorId = doctor.Id,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        DayOfWeek = DayOfWeek.Wednesday,
                        IsAvailable = true,
                        SlotDurationMinutes = 30,
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddMonths(3),
                        Notes = "Regular Wednesday schedule"
                    }
                };

                // Generate time slots for each schedule
                foreach (var schedule in schedules)
                {
                    var slots = new List<TimeSlot>();
                    var startDate = schedule.ValidFrom.Value;
                    var endDate = schedule.ValidTo.Value;

                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek != schedule.DayOfWeek) continue;

                        var slotStart = date.Date.Add(schedule.StartTime);
                        var slotEnd = date.Date.Add(schedule.EndTime);

                        while (slotStart.AddMinutes(schedule.SlotDurationMinutes) <= slotEnd)
                        {
                            slots.Add(new TimeSlot
                            {
                                StartTime = slotStart,
                                EndTime = slotStart.AddMinutes(schedule.SlotDurationMinutes),
                                IsBooked = false
                            });

                            slotStart = slotStart.AddMinutes(schedule.SlotDurationMinutes);
                        }
                    }

                    schedule.TimeSlots = slots;
                }

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