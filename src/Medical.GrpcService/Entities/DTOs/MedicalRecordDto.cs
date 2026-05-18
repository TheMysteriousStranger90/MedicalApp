namespace Medical.GrpcService.Entities.DTOs;

public class MedicalRecordDto
{
    public required string Id { get; set; }
    public required string PatientId { get; set; }
    public required string PatientFullName { get; set; }
    public required string Diagnosis { get; set; }
    public required string Treatment { get; set; }
    public required string Prescriptions { get; set; }
    public required string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<LabResultDto> LabResults { get; set; } = [];
}
