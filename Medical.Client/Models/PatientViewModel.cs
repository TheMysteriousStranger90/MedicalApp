using Google.Protobuf.WellKnownTypes;

namespace Medical.Client.Models;

public class PatientViewModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public Timestamp DateOfBirth { get; set; }
    public DateTime? NextAppointment { get; set; }
    public DateTime? LastAppointment { get; set; }
}