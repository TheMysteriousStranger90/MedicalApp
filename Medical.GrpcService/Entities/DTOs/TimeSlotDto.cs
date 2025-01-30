namespace Medical.GrpcService.Entities.DTOs;

public class TimeSlotDto
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public string? AppointmentId { get; set; }
}