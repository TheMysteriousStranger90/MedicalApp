using Medical.GrpcService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.GrpcService.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Specialization)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .IsRequired();

        builder.Property(d => d.ConsultationFee)
            .HasColumnType("decimal(18,2)");

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Schedules)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}