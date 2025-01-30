using System.ComponentModel.DataAnnotations;

namespace Medical.GrpcService.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    
    [Required]
    public string DoctorId { get; set; }
    
    [Required]
    public DayOfWeek DayOfWeek { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsAvailable { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }

    public virtual Doctor Doctor { get; set; }
    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}