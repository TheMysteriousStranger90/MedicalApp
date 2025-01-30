namespace Medical.Client.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync();
    Task<IEnumerable<DoctorModel>> GetDoctorsBySpecializationAsync(string specialization);
    Task<DoctorModel> GetDoctorByIdAsync(string id);
    Task<IEnumerable<DoctorModel>> GetAvailableDoctorsAsync(DateTime date);
    
    // Schedule management methods
    Task<ScheduleModel> CreateScheduleAsync(CreateScheduleRequest request);
    Task<ScheduleModel> UpdateScheduleAsync(UpdateScheduleRequest request);
    Task<DeleteScheduleResponse> DeleteScheduleAsync(string id);
    Task<IEnumerable<ScheduleModel>> GetDoctorScheduleAsync(string doctorId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<TimeSlotModel>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date);
}