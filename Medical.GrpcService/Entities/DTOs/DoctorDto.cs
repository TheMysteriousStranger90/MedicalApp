namespace Medical.GrpcService.Entities.DTOs;

public class DoctorDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Specialization { get; set; }
    public string LicenseNumber { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
}