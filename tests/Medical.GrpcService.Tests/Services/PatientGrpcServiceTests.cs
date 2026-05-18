using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Entities.Enums;
using Medical.GrpcService.Mapping;
using Medical.GrpcService.Repositories.Interfaces;
using Medical.GrpcService.Services;
using Medical.GrpcService.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Medical.GrpcService.Tests.Services;

public sealed class PatientGrpcServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPatientRepository> _patientRepo;
    private readonly IMapper _mapper;
    private readonly PatientGrpcService _sut;

    public PatientGrpcServiceTests()
    {
        _patientRepo = new Mock<IPatientRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.Patients).Returns(_patientRepo.Object);

        var svc = new ServiceCollection();
        svc.AddLogging();
        svc.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = svc.BuildServiceProvider().GetRequiredService<IMapper>();

        _sut = new PatientGrpcService(
            _unitOfWork.Object,
            _mapper,
            NullLogger<PatientGrpcService>.Instance);
    }

    // ── GetPatients ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPatients_ReturnsAllPatientsForDoctor()
    {
        string doctorId = Guid.NewGuid().ToString();
        var dtos = new List<PatientDto> { CreateDto(), CreateDto() };
        _patientRepo.Setup(r => r.GetPatientsByDoctorAsync(doctorId)).ReturnsAsync(dtos);

        GetPatientsResponse response = await _sut.GetPatients(
            new GetPatientsRequest { DoctorId = doctorId },
            TestServerCallContext.Create());

        Assert.Equal(2, response.Patients.Count);
    }

    [Fact]
    public async Task GetPatients_EmptyResult_ReturnsEmptyList()
    {
        _patientRepo.Setup(r => r.GetPatientsByDoctorAsync(It.IsAny<string>())).ReturnsAsync([]);

        GetPatientsResponse response = await _sut.GetPatients(
            new GetPatientsRequest { DoctorId = Guid.NewGuid().ToString() },
            TestServerCallContext.Create());

        Assert.Empty(response.Patients);
    }

    // ── GetPatientById ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPatientById_Found_ReturnsModel()
    {
        string id = Guid.NewGuid().ToString();
        PatientDto dto = CreateDto(id);
        _patientRepo.Setup(r => r.GetPatientWithMedicalRecordsAsync(id)).ReturnsAsync(dto);

        PatientModel result = await _sut.GetPatientById(
            new GetPatientByIdRequest { Id = id }, TestServerCallContext.Create());

        Assert.Equal(id, result.Id);
        Assert.Equal(dto.FullName, result.FullName);
    }

    [Fact]
    public async Task GetPatientById_NotFound_ThrowsRpcException()
    {
        string id = Guid.NewGuid().ToString();
        _patientRepo.Setup(r => r.GetPatientWithMedicalRecordsAsync(id)).ReturnsAsync((PatientDto?)null);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(() =>
            _sut.GetPatientById(new GetPatientByIdRequest { Id = id }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    // ── GetPatientMedicalHistory ──────────────────────────────────────────────

    [Fact]
    public async Task GetPatientMedicalHistory_NotFound_ThrowsRpcException()
    {
        string id = Guid.NewGuid().ToString();
        _patientRepo.Setup(r => r.GetPatientWithMedicalRecordsAsync(id)).ReturnsAsync((PatientDto?)null);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(() => _sut.GetPatientMedicalHistory(
            new GetPatientByIdRequest { Id = id }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task GetPatientMedicalHistory_NoRecords_ReturnsEmpty()
    {
        string id = Guid.NewGuid().ToString();
        PatientDto dto = CreateDto(id);
        dto.MedicalRecords = [];
        _patientRepo.Setup(r => r.GetPatientWithMedicalRecordsAsync(id)).ReturnsAsync(dto);

        GetMedicalRecordsResponse result = await _sut.GetPatientMedicalHistory(
            new GetPatientByIdRequest { Id = id }, TestServerCallContext.Create());

        Assert.Empty(result.Records);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PatientDto CreateDto(string? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = "patient@example.com",
            UserName = "patient@example.com",
            FullName = "John Doe",
            DateOfBirth = new DateTime(1985, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            Phone = "+1234567890",
            Address = "123 Main St"
        };
}
