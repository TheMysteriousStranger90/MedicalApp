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

public sealed class AppointmentGrpcServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IAppointmentRepository> _appointmentRepo;
    private readonly IMapper _mapper;
    private readonly AppointmentGrpcService _sut;

    public AppointmentGrpcServiceTests()
    {
        _appointmentRepo = new Mock<IAppointmentRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.Appointments).Returns(_appointmentRepo.Object);

        var svc = new ServiceCollection();
        svc.AddLogging();
        svc.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = svc.BuildServiceProvider().GetRequiredService<IMapper>();

        _sut = new AppointmentGrpcService(
            _unitOfWork.Object,
            _mapper,
            NullLogger<AppointmentGrpcService>.Instance);
    }

    // ── GetAppointments ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointments_ByDoctorId_ReturnsAppointments()
    {
        string doctorId = Guid.NewGuid().ToString();
        var dtos = new List<AppointmentDto>
        {
            CreateDto(doctorId)
        };

        _appointmentRepo
            .Setup(r => r.GetDoctorAppointmentsAsync(doctorId))
            .ReturnsAsync(dtos);

        var request = new AppointmentRequest { DoctorId = doctorId };
        AppointmentResponse response = await _sut.GetAppointments(request, TestServerCallContext.Create());

        Assert.Single(response.Appointments);
        Assert.Equal(dtos[0].Id, response.Appointments[0].Id);
    }

    [Fact]
    public async Task GetAppointments_ByPatientId_ReturnsAppointments()
    {
        string patientId = Guid.NewGuid().ToString();
        var dtos = new List<AppointmentDto> { CreateDto(patientId: patientId) };

        _appointmentRepo
            .Setup(r => r.GetPatientAppointmentsAsync(patientId))
            .ReturnsAsync(dtos);

        var request = new AppointmentRequest { PatientId = patientId };
        AppointmentResponse response = await _sut.GetAppointments(request, TestServerCallContext.Create());

        Assert.Single(response.Appointments);
    }

    [Fact]
    public async Task GetAppointments_NoFilter_ReturnsUpcoming()
    {
        var dtos = new List<AppointmentDto> { CreateDto(), CreateDto() };

        _appointmentRepo
            .Setup(r => r.GetUpcomingAppointmentsAsync())
            .ReturnsAsync(dtos);

        AppointmentResponse response =
            await _sut.GetAppointments(new AppointmentRequest(), TestServerCallContext.Create());

        Assert.Equal(2, response.Appointments.Count);
    }

    // ── GetAppointmentById ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointmentById_InvalidGuid_ThrowsRpcException()
    {
        var request = new AppointmentByIdRequest { Id = "not-a-guid" };

        RpcException ex =
            await Assert.ThrowsAsync<RpcException>(() =>
                _sut.GetAppointmentById(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentById_NotFound_ThrowsRpcException()
    {
        string id = Guid.NewGuid().ToString();
        _appointmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Appointment?)null);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(() =>
            _sut.GetAppointmentById(new AppointmentByIdRequest { Id = id }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentById_Found_ReturnsModel()
    {
        var id = Guid.NewGuid();
        Appointment appointment = CreateEntity(id);
        _appointmentRepo.Setup(r => r.GetByIdAsync(id.ToString())).ReturnsAsync(appointment);

        AppointmentModel result = await _sut.GetAppointmentById(
            new AppointmentByIdRequest { Id = id.ToString() },
            TestServerCallContext.Create());

        Assert.Equal(id.ToString(), result.Id);
    }

    // ── CreateAppointment ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAppointment_MissingDoctorId_ThrowsRpcException()
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        RpcException ex =
            await Assert.ThrowsAsync<RpcException>(() =>
                _sut.CreateAppointment(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_DoctorNotFound_ThrowsRpcException()
    {
        string doctorId = Guid.NewGuid().ToString();
        var doctorRepo = new Mock<IDoctorRepository>();
        doctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync((Doctor?)null);
        _unitOfWork.Setup(u => u.Doctors).Returns(doctorRepo.Object);

        var request = new CreateAppointmentRequest
        {
            DoctorId = doctorId,
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        RpcException ex =
            await Assert.ThrowsAsync<RpcException>(() =>
                _sut.CreateAppointment(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_Success_ReturnsModel()
    {
        string doctorId = Guid.NewGuid().ToString();
        var doctor = new Doctor
        {
            Id = doctorId,
            FullName = "Dr. Smith",
            Specialization = "Cardiology",
            LicenseNumber = "LIC-001",
            UserName = "dr.smith@medicalapp.com",
            Email = "dr.smith@medicalapp.com"
        };

        var doctorRepo = new Mock<IDoctorRepository>();
        doctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _unitOfWork.Setup(u => u.Doctors).Returns(doctorRepo.Object);
        _appointmentRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(true);

        var request = new CreateAppointmentRequest
        {
            DoctorId = doctorId,
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        AppointmentModel result = await _sut.CreateAppointment(request, TestServerCallContext.Create());

        Assert.Equal(doctorId, result.DoctorId);
        _appointmentRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
        _unitOfWork.Verify(u => u.Complete(), Times.Once);
    }

    // ── UpdateAppointment ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAppointment_NotFound_ThrowsRpcException()
    {
        string id = Guid.NewGuid().ToString();
        _appointmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Appointment?)null);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(() => _sut.UpdateAppointment(
            new UpdateAppointmentRequest { Id = id, Status = AppointmentStatus.Cancelled },
            TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateAppointment_StatusChange_UpdatesAppointment()
    {
        var id = Guid.NewGuid();
        Appointment entity = CreateEntity(id);
        _appointmentRepo.Setup(r => r.GetByIdAsync(id.ToString())).ReturnsAsync(entity);
        _appointmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Appointment>())).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(true);

        AppointmentModel result = await _sut.UpdateAppointment(
            new UpdateAppointmentRequest
            {
                Id = id.ToString(),
                Status = AppointmentStatus.Cancelled,
                Notes = "Cancelled by patient",
                Symptoms = string.Empty
            },
            TestServerCallContext.Create());

        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
    }

    // ── DeleteAppointment ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAppointment_Found_ReturnsSuccess()
    {
        string id = Guid.NewGuid().ToString();
        _appointmentRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(true);

        DeleteAppointmentResponse result = await _sut.DeleteAppointment(
            new DeleteAppointmentRequest { Id = id }, TestServerCallContext.Create());

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteAppointment_NotFound_ReturnsFailure()
    {
        string id = Guid.NewGuid().ToString();
        _appointmentRepo.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(true);

        DeleteAppointmentResponse result = await _sut.DeleteAppointment(
            new DeleteAppointmentRequest { Id = id }, TestServerCallContext.Create());

        Assert.False(result.Success);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AppointmentDto CreateDto(
        string? doctorId = null,
        string? patientId = null) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            DoctorId = doctorId ?? Guid.NewGuid().ToString(),
            PatientId = patientId ?? Guid.NewGuid().ToString(),
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            Status = AppointmentStatus.Scheduled,
            Notes = string.Empty,
            Symptoms = string.Empty
        };

    private static Appointment CreateEntity(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            DoctorId = Guid.NewGuid().ToString(),
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            Status = AppointmentStatus.Scheduled
        };
}
