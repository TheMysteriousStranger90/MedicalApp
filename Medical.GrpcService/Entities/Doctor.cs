using System.ComponentModel.DataAnnotations;

namespace Medical.GrpcService.Entities;

public class Doctor : User
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    public string Specialization { get; set; }

    [Required]
    public string LicenseNumber { get; set; }

    public string? Education { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}