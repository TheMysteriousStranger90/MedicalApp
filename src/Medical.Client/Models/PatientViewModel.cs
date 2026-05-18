using Google.Protobuf.WellKnownTypes;

namespace Medical.Client.Models;

public class PatientViewModel
{
    public required string Id { get; set; }
    public required string FullName { get; set; }
    public required string Phone { get; set; }
    public Timestamp DateOfBirth { get; set; } = null!;
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
