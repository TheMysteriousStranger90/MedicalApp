using Google.Protobuf.WellKnownTypes;

namespace Medical.Client.Models;

public class PatientViewModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public Timestamp DateOfBirth { get; set; }
    private DateTime? _nextAppointment;
    private DateTime? _lastAppointment;

    public DateTime? NextAppointment
    {
        get => _nextAppointment?.ToLocalTime();
        set => _nextAppointment = value;
    }

    public DateTime? LastAppointment
    {
        get => _lastAppointment?.ToLocalTime();
        set => _lastAppointment = value;
    }
}