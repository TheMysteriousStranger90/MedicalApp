using System.ComponentModel.DataAnnotations;

namespace Medical.GrpcService.Entities;

public class LabResult
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid MedicalRecordId { get; set; }
    
    [Required]
    public string TestName { get; set; }
    public string TestResult { get; set; }
    public string ReferenceRange { get; set; }
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Comments { get; set; }
    public string? DocumentUrl { get; set; }
    
    public virtual MedicalRecord MedicalRecord { get; set; }
}