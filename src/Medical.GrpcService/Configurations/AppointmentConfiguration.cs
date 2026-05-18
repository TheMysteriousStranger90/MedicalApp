using Medical.GrpcService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.GrpcService.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.DoctorId)
            .IsRequired();

        builder.Property(a => a.PatientId)
            .IsRequired();

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.Fee)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(a => a.AppointmentDate);
    }
}