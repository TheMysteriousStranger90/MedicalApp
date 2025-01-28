namespace Medical.Client.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync();
    Task<IEnumerable<DoctorModel>> GetDoctorsBySpecializationAsync(string specialization);
    Task<DoctorModel> GetDoctorByIdAsync(string id);
    Task<IEnumerable<DoctorModel>> GetAvailableDoctorsAsync(DateTime date);
}