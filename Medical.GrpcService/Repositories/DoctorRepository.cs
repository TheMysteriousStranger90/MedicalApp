using AutoMapper;
using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorRepository> _logger;

    public DoctorRepository(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<DoctorRepository> logger) : base(context)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization)
    {
        try
        {
            _logger.LogInformation("Getting doctors by specialization: {Specialization}", specialization);
            var doctors = await _context.Doctors
                .Where(d => d.Specialization == specialization && d.IsActive)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors by specialization {Specialization}", specialization);
            throw;
        }
    }

    public async Task<DoctorDto?> GetDoctorWithSchedulesAsync(string doctorId)
    {
        try
        {
            _logger.LogInformation("Getting doctor with schedules: {DoctorId}", doctorId);
            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            return _mapper.Map<DoctorDto>(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor with schedules {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(DateTime date)
    {
        try
        {
            _logger.LogInformation("Getting available doctors for date: {Date}", date);
            var doctors = await _context.Doctors
                .Include(d => d.Schedules)
                .Where(d => d.IsActive && d.Schedules.Any(s => 
                    s.DayOfWeek == date.DayOfWeek && 
                    s.IsAvailable &&
                    s.StartTime.TimeOfDay <= date.TimeOfDay &&
                    s.EndTime.TimeOfDay >= date.TimeOfDay))
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available doctors for date {Date}", date);
            throw;
        }
    }
    
    public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all active doctors");
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all doctors");
            throw;
        }
    }
}