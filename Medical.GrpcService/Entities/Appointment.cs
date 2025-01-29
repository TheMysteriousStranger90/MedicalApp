using System.ComponentModel.DataAnnotations;
using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    [Required]
    public string DoctorId { get; set; }

    [Required]
    public string PatientId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public double Fee { get; set; }
    public bool IsPaid { get; set; }
    public string? CancellationReason { get; set; }

    public virtual Doctor Doctor { get; set; }
    public virtual Patient Patient { get; set; }
    
    public Guid? MedicalRecordId { get; set; }
    public virtual MedicalRecord? MedicalRecord { get; set; }
}