using System.ComponentModel.DataAnnotations;

namespace Medical.GrpcService.Entities;

public class Doctor : User
{
    [Required] [StringLength(100)] public required string FullName { get; set; }

    [Required] public required string Specialization { get; set; }

    [Required] public required string LicenseNumber { get; set; }

    public string? Education { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = [];
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
}
