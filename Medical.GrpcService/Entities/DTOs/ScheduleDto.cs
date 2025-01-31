namespace Medical.GrpcService.Entities.DTOs;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
    
    public DoctorDto Doctor { get; set; }
    public ICollection<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
}