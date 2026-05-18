using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AutoMapper;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Entities.Enums;
using Medical.GrpcService.Mapping;

namespace Medical.GrpcService.Tests.Mapping;

public sealed class AutoMapperProfileTests
{
    private readonly IMapper _mapper;

    public AutoMapperProfileTests()
    {
        var svc = new ServiceCollection();
        svc.AddLogging();
        svc.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = svc.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    // ── Appointment ───────────────────────────────────────────────────────────

    [Fact]
    public void Appointment_MapsTo_AppointmentDto()
    {
        var id = Guid.NewGuid();
        var entity = new Appointment
        {
            Id = id,
            DoctorId = Guid.NewGuid().ToString(),
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Scheduled,
            Fee = 100.0,
            IsPaid = false
        };

        AppointmentDto? dto = _mapper.Map<AppointmentDto>(entity);

        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal(entity.DoctorId, dto.DoctorId);
        Assert.Equal(entity.PatientId, dto.PatientId);
        Assert.Equal(entity.AppointmentDate, dto.AppointmentDate);
        Assert.Equal(AppointmentStatus.Scheduled, dto.Status);
    }

    [Fact]
    public void Appointment_MapsTo_AppointmentModel()
    {
        var id = Guid.NewGuid();
        var entity = new Appointment
        {
            Id = id,
            DoctorId = Guid.NewGuid().ToString(),
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Completed
        };

        AppointmentModel? model = _mapper.Map<AppointmentModel>(entity);

        Assert.Equal(id.ToString(), model.Id);
        Assert.Equal(entity.DoctorId, model.DoctorId);
        Assert.Equal(AppointmentStatus.Completed, model.Status);
        // Timestamp round-trip
        Assert.Equal(entity.AppointmentDate, model.AppointmentDate.ToDateTime());
    }

    // ── Doctor ────────────────────────────────────────────────────────────────

    [Fact]
    public void Doctor_MapsTo_DoctorDto()
    {
        string id = Guid.NewGuid().ToString();
        var doctor = new Doctor
        {
            Id = id,
            FullName = "Dr. House",
            Specialization = "Diagnostics",
            LicenseNumber = "LIC-999",
            Email = "house@med.com",
            UserName = "house@med.com"
        };

        DoctorDto? dto = _mapper.Map<DoctorDto>(doctor);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Dr. House", dto.FullName);
        Assert.Equal("Diagnostics", dto.Specialization);
    }

    [Fact]
    public void DoctorDto_MapsTo_DoctorModel()
    {
        var dto = new DoctorDto
        {
            Id = Guid.NewGuid().ToString(),
            Email = "house@med.com",
            UserName = "house@med.com",
            FullName = "Dr. House",
            Specialization = "Diagnostics",
            LicenseNumber = "LIC-999"
        };

        DoctorModel? model = _mapper.Map<DoctorModel>(dto);

        Assert.Equal(dto.Id, model.Id);
        Assert.Equal(dto.FullName, model.FullName);
        Assert.Equal(dto.Specialization, model.Specialization);
    }

    // ── MedicalRecord ─────────────────────────────────────────────────────────

    [Fact]
    public void MedicalRecord_MapsTo_MedicalRecordModel()
    {
        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid().ToString(),
            Diagnosis = "Flu",
            Treatment = "Rest",
            Prescriptions = "None",
            Notes = "N/A",
            CreatedAt = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        MedicalRecordModel? model = _mapper.Map<MedicalRecordModel>(record);

        Assert.Equal(record.PatientId, model.PatientId);
        Assert.Equal("Flu", model.Diagnosis);
        Assert.Equal(record.CreatedAt, model.CreatedAt.ToDateTime());
    }

    [Fact]
    public void CreateMedicalRecordRequest_MapsTo_MedicalRecord()
    {
        string patientId = Guid.NewGuid().ToString();
        var request = new CreateMedicalRecordRequest
        {
            PatientId = patientId,
            Diagnosis = "Hypertension",
            Treatment = "Medication",
            Prescriptions = "Amlodipine",
            Notes = "Follow up"
        };

        MedicalRecord? entity = _mapper.Map<MedicalRecord>(request);

        Assert.Equal(patientId, entity.PatientId);
        Assert.Equal("Hypertension", entity.Diagnosis);
        Assert.NotEqual(Guid.Empty, entity.Id);
    }
}
