using System.ComponentModel.DataAnnotations;

namespace Medical.Client.Models;

public class CompleteAppointmentModel
{
    [Required]
    public string AppointmentId { get; set; }
    
    [Required]
    public string Diagnosis { get; set; }
    
    [Required]
    public string Treatment { get; set; }
    
    public string? Prescriptions { get; set; }
    
    public string? Notes { get; set; }
}