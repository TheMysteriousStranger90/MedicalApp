using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization);
    Task<DoctorDto?> GetDoctorWithSchedulesAsync(string doctorId);
    Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync(DateTime date);
}