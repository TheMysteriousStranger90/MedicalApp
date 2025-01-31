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
                .ThenInclude(s => s.TimeSlots.Where(ts => ts.StartTime >= DateTime.Today))
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsActive);

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
            var timeOfDay = date.TimeOfDay;

            var doctors = await _context.Doctors
                .Include(d => d.Schedules)
                .ThenInclude(s => s.TimeSlots)
                .Where(d => d.IsActive && d.Schedules.Any(s =>
                    s.DayOfWeek == date.DayOfWeek &&
                    s.IsAvailable &&
                    s.StartTime <= timeOfDay &&
                    s.EndTime >= timeOfDay &&
                    s.TimeSlots.Any(ts =>
                        ts.StartTime.Date == date.Date &&
                        !ts.IsBooked)))
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
    
    public async Task<ScheduleDto> CreateScheduleAsync(Schedule schedule)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (schedule.EndTime <= schedule.StartTime)
                throw new ArgumentException("End time must be after start time");

            if (schedule.SlotDurationMinutes <= 0 || schedule.SlotDurationMinutes > 120)
                throw new ArgumentException("Invalid slot duration");

            var existingSchedule = await _context.Schedules
                .AnyAsync(s => s.DoctorId == schedule.DoctorId
                               && s.DayOfWeek == schedule.DayOfWeek
                               && ((s.StartTime <= schedule.StartTime && s.EndTime > schedule.StartTime)
                                   || (s.StartTime < schedule.EndTime && s.EndTime >= schedule.EndTime))
                               && (s.ValidTo == null || s.ValidTo >= schedule.ValidFrom));

            if (existingSchedule)
                throw new InvalidOperationException("Schedule overlaps with existing schedule");

            schedule.Id = Guid.NewGuid();
            schedule.TimeSlots = GenerateTimeSlots(schedule);

            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<ScheduleDto>(schedule);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating schedule for doctor {DoctorId}", schedule.DoctorId);
            throw;
        }
    }

    private ICollection<TimeSlot> GenerateTimeSlots(Schedule schedule)
    {
        try
        {
            var slots = new List<TimeSlot>();
            var startDate = schedule.ValidFrom ?? DateTime.Today;
            var endDate = schedule.ValidTo ?? startDate.AddMonths(3);

            _logger.LogInformation(
                "Generating slots for schedule {ScheduleId} from {StartDate} to {EndDate}, Duration: {Duration}min",
                schedule.Id, startDate, endDate, schedule.SlotDurationMinutes);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != schedule.DayOfWeek) continue;

                var slotStart = date.Date.Add(schedule.StartTime);
                var slotEnd = date.Date.Add(schedule.EndTime);

                while (slotStart.AddMinutes(schedule.SlotDurationMinutes) <= slotEnd)
                {
                    slots.Add(new TimeSlot
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = schedule.Id,
                        StartTime = slotStart,
                        EndTime = slotStart.AddMinutes(schedule.SlotDurationMinutes),
                        IsBooked = false
                    });

                    slotStart = slotStart.AddMinutes(schedule.SlotDurationMinutes);
                }
            }

            _logger.LogInformation("Generated {Count} slots", slots.Count);
            return slots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating time slots");
            throw;
        }
    }

    public async Task<bool> IsTimeSlotAvailable(Guid scheduleId, DateTime startTime)
    {
        return await _context.TimeSlots
            .Where(ts => ts.ScheduleId == scheduleId)
            .AnyAsync(ts => ts.StartTime == startTime && !ts.IsBooked);
    }

    public async Task<Schedule?> GetScheduleByIdAsync(string id)
    {
        try
        {
            return await _context.Schedules
                .Include(s => s.TimeSlots)
                .FirstOrDefaultAsync(s => s.Id.ToString() == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule by id {ScheduleId}", id);
            throw;
        }
    }

    public async Task<ScheduleDto> UpdateScheduleAsync(Schedule schedule)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.Schedules
                .Include(s => s.TimeSlots)
                .FirstOrDefaultAsync(s => s.Id == schedule.Id);

            if (existing == null)
                throw new InvalidOperationException($"Schedule {schedule.Id} not found");

            _context.Entry(existing).State = EntityState.Detached;
            
            _context.Schedules.Attach(schedule);
            _context.Entry(schedule).State = EntityState.Modified;
            
            var slots = await _context.TimeSlots
                .Where(ts => ts.ScheduleId == schedule.Id)
                .ToListAsync();
            
            _context.TimeSlots.RemoveRange(slots);
            await _context.SaveChangesAsync();

            // Generate new slots
            schedule.TimeSlots = GenerateTimeSlots(schedule);
            await _context.TimeSlots.AddRangeAsync(schedule.TimeSlots);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return _mapper.Map<ScheduleDto>(schedule);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Concurrency conflict updating schedule {Id}", schedule.Id);
            throw new InvalidOperationException("Schedule was modified by another user");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating schedule {Id}", schedule.Id);
            throw;
        }
    }

    public async Task<bool> DeleteScheduleAsync(string id)
    {
        try
        {
            var schedule = await _context.Schedules.FindAsync(Guid.Parse(id));
            if (schedule == null) return false;

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ScheduleDto>> GetDoctorSchedulesAsync(string doctorId, DateTime fromDate,
        DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting schedules for doctor {DoctorId} from {FromDate} to {ToDate}",
                doctorId, fromDate, toDate);

            var schedules = await _context.Schedules
                .Include(s => s.TimeSlots.Where(ts =>
                    ts.StartTime >= fromDate &&
                    ts.StartTime <= toDate))
                .Where(s => s.DoctorId == doctorId &&
                            (s.ValidTo == null || s.ValidTo >= fromDate) &&
                            (s.ValidFrom == null || s.ValidFrom <= toDate))
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedules for doctor {DoctorId}", doctorId);
            throw;
        }
    }
}