using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IPatientRepository : IGenericRepository<Patient>
{
    public Task<PatientDto?> GetPatientWithMedicalRecordsAsync(string patientId);
    public Task<IEnumerable<PatientDto>> GetPatientsByDoctorAsync(string doctorId);
}
