using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    public Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization);
    public Task<DoctorDto?> GetDoctorWithSchedulesAsync(string doctorId);
    public Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(DateTime date);
    public Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
    public Task<ScheduleDto> CreateScheduleAsync(Schedule schedule);
    public Task<bool> IsTimeSlotAvailable(Guid scheduleId, DateTime startTime);
    public Task<Schedule?> GetScheduleByIdAsync(string id);
    public Task<ScheduleDto> UpdateScheduleAsync(Schedule schedule);
    public Task<bool> DeleteScheduleAsync(string id);
    public Task<IEnumerable<ScheduleDto>> GetDoctorSchedulesAsync(string doctorId, DateTime fromDate, DateTime toDate);
}
