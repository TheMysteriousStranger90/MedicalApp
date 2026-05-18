using System.ComponentModel.DataAnnotations;

namespace Medical.Client.Models;

public class CompleteAppointmentModel
{
    [Required] public required string AppointmentId { get; set; }

    [Required] public required string Diagnosis { get; set; }

    [Required] public required string Treatment { get; set; }

    public string? Prescriptions { get; set; }

    public string? Notes { get; set; }
}
