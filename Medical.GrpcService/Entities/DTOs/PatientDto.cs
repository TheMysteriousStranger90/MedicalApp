using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities.DTOs;

public class PatientDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public ICollection<MedicalRecordDto> MedicalRecords { get; set; } = new List<MedicalRecordDto>();
}