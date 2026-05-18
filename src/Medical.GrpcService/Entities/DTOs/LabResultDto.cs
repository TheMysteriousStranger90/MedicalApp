namespace Medical.GrpcService.Entities.DTOs;

public class LabResultDto
{
    public required string Id { get; set; }
    public required string TestName { get; set; }
    public required string TestResult { get; set; }
    public required string ReferenceRange { get; set; }
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public bool IsAbnormal { get; set; }
    public string? Comments { get; set; }
    public string? DocumentUrl { get; set; }
}
