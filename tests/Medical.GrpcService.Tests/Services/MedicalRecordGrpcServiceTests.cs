using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Mapping;
using Medical.GrpcService.Repositories.Interfaces;
using Medical.GrpcService.Services;
using Medical.GrpcService.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Medical.GrpcService.Tests.Services;

public sealed class MedicalRecordGrpcServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMedicalRecordRepository> _recordRepo;
    private readonly IMapper _mapper;
    private readonly MedicalRecordGrpcService _sut;

    public MedicalRecordGrpcServiceTests()
    {
        _recordRepo = new Mock<IMedicalRecordRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.MedicalRecords).Returns(_recordRepo.Object);

        var svc = new ServiceCollection();
        svc.AddLogging();
        svc.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = svc.BuildServiceProvider().GetRequiredService<IMapper>();

        _sut = new MedicalRecordGrpcService(
            _unitOfWork.Object,
            _mapper,
            NullLogger<MedicalRecordGrpcService>.Instance);
    }

    // ── GetMedicalRecords ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMedicalRecords_ReturnsRecordsForPatient()
    {
        string patientId = Guid.NewGuid().ToString();
        var dtos = new List<MedicalRecordDto>
        {
            CreateDto(patientId),
            CreateDto(patientId)
        };

        _recordRepo
            .Setup(r => r.GetPatientMedicalHistoryAsync(patientId))
            .ReturnsAsync(dtos);

        GetMedicalRecordsResponse response = await _sut.GetMedicalRecords(
            new GetMedicalRecordsRequest { PatientId = patientId },
            TestServerCallContext.Create());

        Assert.Equal(2, response.Records.Count);
    }

    [Fact]
    public async Task GetMedicalRecords_EmptyHistory_ReturnsEmpty()
    {
        string patientId = Guid.NewGuid().ToString();
        _recordRepo
            .Setup(r => r.GetPatientMedicalHistoryAsync(patientId))
            .ReturnsAsync([]);

        GetMedicalRecordsResponse response = await _sut.GetMedicalRecords(
            new GetMedicalRecordsRequest { PatientId = patientId },
            TestServerCallContext.Create());

        Assert.Empty(response.Records);
    }

    // ── CreateMedicalRecord ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateMedicalRecord_Success_ReturnsModel()
    {
        string patientId = Guid.NewGuid().ToString();
        _recordRepo.Setup(r => r.AddAsync(It.IsAny<MedicalRecord>())).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(true);

        var request = new CreateMedicalRecordRequest
        {
            PatientId = patientId,
            Diagnosis = "Hypertension",
            Treatment = "Amlodipine 5mg",
            Prescriptions = "Take daily",
            Notes = "Follow up in 4 weeks"
        };

        MedicalRecordModel result = await _sut.CreateMedicalRecord(request, TestServerCallContext.Create());

        Assert.Equal(patientId, result.PatientId);
        Assert.Equal("Hypertension", result.Diagnosis);
        _recordRepo.Verify(r => r.AddAsync(It.IsAny<MedicalRecord>()), Times.Once);
        _unitOfWork.Verify(u => u.Complete(), Times.Once);
    }

    [Fact]
    public async Task CreateMedicalRecord_SaveFails_ThrowsRpcException()
    {
        _recordRepo.Setup(r => r.AddAsync(It.IsAny<MedicalRecord>())).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(false);

        var request = new CreateMedicalRecordRequest
        {
            PatientId = Guid.NewGuid().ToString(),
            Diagnosis = "Test",
            Treatment = "Test",
            Prescriptions = string.Empty,
            Notes = string.Empty
        };

        RpcException ex =
            await Assert.ThrowsAsync<RpcException>(() =>
                _sut.CreateMedicalRecord(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.Internal, ex.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static MedicalRecordDto CreateDto(string? patientId = null) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            PatientId = patientId ?? Guid.NewGuid().ToString(),
            PatientFullName = "John Doe",
            Diagnosis = "Test diagnosis",
            Treatment = "Test treatment",
            Prescriptions = "None",
            Notes = "No notes",
            CreatedAt = DateTime.UtcNow
        };
}
