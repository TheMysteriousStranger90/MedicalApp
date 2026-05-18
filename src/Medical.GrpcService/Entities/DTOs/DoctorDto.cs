namespace Medical.GrpcService.Entities.DTOs;

public class DoctorDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public required string Specialization { get; set; }
    public required string LicenseNumber { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
}
