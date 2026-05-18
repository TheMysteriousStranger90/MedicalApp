using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<PatientDto?> GetPatientWithMedicalRecordsAsync(string patientId);
    Task<IEnumerable<PatientDto>> GetPatientsByDoctorAsync(string doctorId);
}