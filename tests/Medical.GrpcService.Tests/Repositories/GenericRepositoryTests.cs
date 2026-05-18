using Xunit;
using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.Enums;
using Medical.GrpcService.Repositories;
using Medical.GrpcService.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Tests.Repositories;

/// <summary>
/// Tests for GenericRepository using the EF Core InMemory provider.
/// Each test gets a fresh database to guarantee isolation.
/// </summary>
public sealed class GenericRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GenericRepository<Appointment> _repository;

    public GenericRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new GenericRepository<Appointment>(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsEntity()
    {
        var appointment = CreateAppointment();
        await _repository.AddAsync(appointment);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(appointment.Id.ToString());

        Assert.NotNull(result);
        Assert.Equal(appointment.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidGuid_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync("not-a-guid");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownGuid_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        await _repository.AddAsync(CreateAppointment());
        await _repository.AddAsync(CreateAppointment());
        await _context.SaveChangesAsync();

        var all = await _repository.GetAllAsync();

        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyDb_ReturnsEmpty()
    {
        var all = await _repository.GetAllAsync();

        Assert.Empty(all);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesArePersisted()
    {
        var appointment = CreateAppointment();
        await _repository.AddAsync(appointment);
        await _context.SaveChangesAsync();

        appointment.Status = AppointmentStatus.Cancelled;
        await _repository.UpdateAsync(appointment);
        await _context.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(appointment.Id.ToString());
        Assert.NotNull(updated);
        Assert.Equal(AppointmentStatus.Cancelled, updated.Status);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingEntity_ReturnsTrue()
    {
        var appointment = CreateAppointment();
        await _repository.AddAsync(appointment);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(appointment.Id.ToString());
        await _context.SaveChangesAsync();

        Assert.True(result);
        Assert.Null(await _repository.GetByIdAsync(appointment.Id.ToString()));
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid().ToString());

        Assert.False(result);
    }

    // ── Exists ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Exists_AfterAdd_ReturnsTrue()
    {
        var appointment = CreateAppointment();
        await _repository.AddAsync(appointment);
        await _context.SaveChangesAsync();

        var exists = await _repository.Exists(appointment.Id.ToString());

        Assert.True(exists);
    }

    [Fact]
    public async Task Exists_UnknownId_ReturnsFalse()
    {
        var exists = await _repository.Exists(Guid.NewGuid().ToString());

        Assert.False(exists);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Appointment CreateAppointment() =>
        new()
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid().ToString(),
            PatientId = Guid.NewGuid().ToString(),
            AppointmentDate = DateTime.UtcNow.AddDays(3),
            Status = AppointmentStatus.Scheduled
        };
}