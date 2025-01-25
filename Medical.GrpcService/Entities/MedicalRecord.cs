namespace Medical.GrpcService.Entities;

public class MedicalRecord
{
    public Guid Id { get; set; }
    public string PatientId { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Prescriptions { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Patient Patient { get; set; }
    public virtual ICollection<LabResult> LabResults { get; set; }
}