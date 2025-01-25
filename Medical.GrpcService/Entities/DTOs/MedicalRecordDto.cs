namespace Medical.GrpcService.Entities.DTOs;

public class MedicalRecordDto
{
    public string Id { get; set; }
    public string PatientId { get; set; }
    public string PatientFullName { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Prescriptions { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<LabResultDto> LabResults { get; set; } = new List<LabResultDto>();
}