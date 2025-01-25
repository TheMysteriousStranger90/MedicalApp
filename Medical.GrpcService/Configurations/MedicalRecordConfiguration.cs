using Medical.GrpcService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.GrpcService.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.Property(mr => mr.PatientId)
            .IsRequired();

        builder.HasMany(mr => mr.LabResults)
            .WithOne(lr => lr.MedicalRecord)
            .HasForeignKey(lr => lr.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mr => mr.CreatedAt);
    }
}