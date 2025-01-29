namespace Medical.Client.Models;

public class AppointmentViewModel
{
    public string Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string DoctorName { get; set; }
    public string PatientName { get; set; }
    public AppointmentStatus Status { get; set; }
}