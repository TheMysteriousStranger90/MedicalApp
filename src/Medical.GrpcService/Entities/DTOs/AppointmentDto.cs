using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities.DTOs;

public class AppointmentDto
{
    public required string Id { get; set; }
    public required string DoctorId { get; set; }
    public required string PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }
    public required string Notes { get; set; }
    public required string Symptoms { get; set; }
    public double Fee { get; set; }
    public bool IsPaid { get; set; }
}
