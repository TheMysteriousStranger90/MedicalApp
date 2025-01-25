using Medical.GrpcService.Entities.Enums;

namespace Medical.GrpcService.Entities.DTOs;

public class AppointmentDto
{
    public string Id { get; set; }
    public string DoctorId { get; set; }
    public string PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; }
    public string Symptoms { get; set; }
    public double Fee { get; set; }
    public bool IsPaid { get; set; }
}