namespace Medical.GrpcService.Entities;

public class MedicalRecord
{
    public Guid Id { get; set; }
    public required string PatientId { get; set; }
    public required string Diagnosis { get; set; }
    public required string Treatment { get; set; }
    public required string Prescriptions { get; set; }
    public required string Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
    public virtual ICollection<LabResult> LabResults { get; set; } = [];
}
