namespace Medical.Client.Models;

public class AppointmentViewModel
{
    public required string Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public required string DoctorName { get; set; }
    public required string PatientName { get; set; }
    public AppointmentStatus Status { get; set; }
}
