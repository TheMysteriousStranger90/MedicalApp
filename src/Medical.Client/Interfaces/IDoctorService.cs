using Google.Protobuf.WellKnownTypes;

namespace Medical.Client.Interfaces;

public interface IDoctorService
{
    public Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync();
    public Task<IEnumerable<DoctorModel>> GetDoctorsBySpecializationAsync(string specialization);
    public Task<DoctorModel> GetDoctorByIdAsync(string id);
    public Task<IEnumerable<DoctorModel>> GetAvailableDoctorsAsync(DateTime date);

    // Schedule management methods
    public Task<ScheduleModel> CreateScheduleAsync(CreateScheduleRequest request);
    public Task<ScheduleModel> UpdateScheduleAsync(UpdateScheduleRequest request);
    public Task<DeleteScheduleResponse> DeleteScheduleAsync(string id);
    public Task<IEnumerable<ScheduleModel>> GetDoctorScheduleAsync(string doctorId, DateTime fromDate, DateTime toDate);
    public Task<IEnumerable<TimeSlotModel>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date);
}
