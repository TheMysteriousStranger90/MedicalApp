using AutoMapper;
using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentRepository> _logger;

    public AppointmentRepository(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<AppointmentRepository> logger) : base(context)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(string doctorId)
    {
        try
        {
            _logger.LogInformation("Getting appointments for doctor: {DoctorId}", doctorId);
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor appointments for {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientId)
    {
        try
        {
            _logger.LogInformation("Getting appointments for patient: {PatientId}", patientId);
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient appointments for {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date)
    {
        try
        {
            _logger.LogInformation("Getting appointments for date: {Date}", date.ToShortDateString());
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointments for date {Date}", date.ToShortDateString());
            throw;
        }
    }

    public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync()
    {
        try
        {
            _logger.LogInformation("Getting upcoming appointments");
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate > DateTime.UtcNow && a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming appointments");
            throw;
        }
    }
}