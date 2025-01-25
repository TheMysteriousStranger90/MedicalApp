using Medical.GrpcService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.GrpcService.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.Property(lr => lr.MedicalRecordId)
            .IsRequired();

        builder.Property(lr => lr.TestName)
            .IsRequired();

        builder.HasIndex(lr => lr.TestDate);
    }
}