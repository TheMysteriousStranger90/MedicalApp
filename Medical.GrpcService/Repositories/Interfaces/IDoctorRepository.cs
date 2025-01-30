using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization);
    Task<DoctorDto?> GetDoctorWithSchedulesAsync(string doctorId);
    Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(DateTime date);
    Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
    Task<ScheduleDto> CreateScheduleAsync(Schedule schedule);
    Task<bool> IsTimeSlotAvailable(Guid scheduleId, DateTime startTime);
    Task<Schedule?> GetScheduleByIdAsync(string id);
    Task<ScheduleDto> UpdateScheduleAsync(Schedule schedule);
    Task<bool> DeleteScheduleAsync(string id);
    Task<IEnumerable<ScheduleDto>> GetDoctorSchedulesAsync(string doctorId, DateTime fromDate, DateTime toDate);
}