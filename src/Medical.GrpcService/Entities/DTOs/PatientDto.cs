using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities.DTOs;

public class PatientDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public required string Phone { get; set; }
    public required string Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public ICollection<MedicalRecordDto> MedicalRecords { get; set; } = [];
}
