using System.ComponentModel.DataAnnotations;

namespace Medical.GrpcService.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    
    [Required]
    public string DoctorId { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    public bool IsAvailable { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int MaxAppointments { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }

    public virtual Doctor Doctor { get; set; }
}