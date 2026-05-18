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

public sealed class DoctorGrpcServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IDoctorRepository> _doctorRepo;
    private readonly IMapper _mapper;
    private readonly DoctorGrpcService _sut;

    public DoctorGrpcServiceTests()
    {
        _doctorRepo = new Mock<IDoctorRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.Doctors).Returns(_doctorRepo.Object);

        var svc = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        svc.AddLogging();
        svc.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = svc.BuildServiceProvider().GetRequiredService<IMapper>();

        _sut = new DoctorGrpcService(
            _unitOfWork.Object,
            _mapper,
            NullLogger<DoctorGrpcService>.Instance);
    }

    // ── GetDoctors ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDoctors_NoFilter_ReturnsAllDoctors()
    {
        var dtos = new List<DoctorDto> { CreateDto(), CreateDto() };
        _doctorRepo.Setup(r => r.GetAllDoctorsAsync()).ReturnsAsync(dtos);

        var response = await _sut.GetDoctors(new GetDoctorsRequest(), TestServerCallContext.Create());

        Assert.Equal(2, response.Doctors.Count);
        _doctorRepo.Verify(r => r.GetAllDoctorsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDoctors_WithSpecialization_FiltersResults()
    {
        const string spec = "Cardiology";
        var dtos = new List<DoctorDto> { CreateDto(specialization: spec) };
        _doctorRepo.Setup(r => r.GetDoctorsBySpecializationAsync(spec)).ReturnsAsync(dtos);

        var response = await _sut.GetDoctors(
            new GetDoctorsRequest { Specialization = spec },
            TestServerCallContext.Create());

        Assert.Single(response.Doctors);
        _doctorRepo.Verify(r => r.GetDoctorsBySpecializationAsync(spec), Times.Once);
    }

    [Fact]
    public async Task GetDoctors_EmptyResult_ReturnsEmptyList()
    {
        _doctorRepo.Setup(r => r.GetAllDoctorsAsync()).ReturnsAsync([]);

        var response = await _sut.GetDoctors(new GetDoctorsRequest(), TestServerCallContext.Create());

        Assert.Empty(response.Doctors);
    }

    // ── GetDoctorById ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDoctorById_Found_ReturnsModel()
    {
        var id = Guid.NewGuid().ToString();
        var dto = CreateDto(id: id);
        _doctorRepo.Setup(r => r.GetDoctorWithSchedulesAsync(id)).ReturnsAsync(dto);

        var result = await _sut.GetDoctorById(
            new GetDoctorByIdRequest { Id = id }, TestServerCallContext.Create());

        Assert.Equal(id, result.Id);
        Assert.Equal(dto.FullName, result.FullName);
    }

    [Fact]
    public async Task GetDoctorById_NotFound_ThrowsRpcException()
    {
        var id = Guid.NewGuid().ToString();
        _doctorRepo.Setup(r => r.GetDoctorWithSchedulesAsync(id)).ReturnsAsync((DoctorDto?)null);

        var ex = await Assert.ThrowsAsync<RpcException>(
            () => _sut.GetDoctorById(new GetDoctorByIdRequest { Id = id }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    // ── GetAvailableDoctors ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableDoctors_ReturnsAvailableDoctors()
    {
        var date = DateTime.UtcNow;
        var dtos = new List<DoctorDto> { CreateDto() };
        _doctorRepo.Setup(r => r.GetAvailableDoctorsAsync(It.IsAny<DateTime>())).ReturnsAsync(dtos);

        var response = await _sut.GetAvailableDoctors(
            new GetAvailableDoctorsRequest
            {
                Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(date)
            },
            TestServerCallContext.Create());

        Assert.Single(response.Doctors);
    }

    // ── DeleteSchedule ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSchedule_Success_ReturnsSuccess()
    {
        var id = Guid.NewGuid().ToString();
        _doctorRepo.Setup(r => r.DeleteScheduleAsync(id)).ReturnsAsync(true);

        var result = await _sut.DeleteSchedule(
            new DeleteScheduleRequest { Id = id }, TestServerCallContext.Create());

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteSchedule_NotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid().ToString();
        _doctorRepo.Setup(r => r.DeleteScheduleAsync(id)).ReturnsAsync(false);

        var result = await _sut.DeleteSchedule(
            new DeleteScheduleRequest { Id = id }, TestServerCallContext.Create());

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DoctorDto CreateDto(string? id = null, string? specialization = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = "doctor@medicalapp.com",
            UserName = "doctor@medicalapp.com",
            FullName = "Dr. House",
            Specialization = specialization ?? "General",
            LicenseNumber = "LIC-001"
        };
}