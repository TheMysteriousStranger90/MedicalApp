using System.Diagnostics.CodeAnalysis;
using Medical.GrpcService.Configurations;
using Medical.GrpcService.Entities;
using Medical.GrpcService.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Medical.GrpcService.Context;

public class ApplicationDbContext : IdentityDbContext<User, Role, string,
    IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<LabResult> LabResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalRecordConfiguration());
        modelBuilder.ApplyConfiguration(new LabResultConfiguration());
    }

    [ExcludeFromCodeCoverage]
    public async Task MigrateAndCreateDataAsync(IServiceProvider serviceProvider)
    {
        var retryPolicy = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(15)
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
            await Database.MigrateAsync();

            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            await SeedDataInitializer.SeedUsersAsync(userManager, roleManager);
            await SeedDataInitializer.SeedDataAsync(this);
            await SaveChangesAsync();
        });
    }
}