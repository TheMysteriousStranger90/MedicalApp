namespace Medical.GrpcService.Entities.DTOs;

public class LabResultDto
{
    public string Id { get; set; }
    public string TestName { get; set; }
    public string TestResult { get; set; }
    public string ReferenceRange { get; set; }
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Comments { get; set; }
    public string? DocumentUrl { get; set; }
}