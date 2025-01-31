namespace Medical.GrpcService.Entities;

public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public string? AppointmentId { get; set; }

    public virtual Schedule Schedule { get; set; }
    public virtual Appointment? Appointment { get; set; }
}