using System.ComponentModel.DataAnnotations;
using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities;

public class Patient : User
{
    [Required]
    [StringLength(100)]
    public required string FullName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Phone]
    public required string Phone { get; set; }

    public required string Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
}
