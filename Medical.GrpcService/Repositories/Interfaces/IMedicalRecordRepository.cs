using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
{
    Task<IEnumerable<MedicalRecordDto>> GetPatientMedicalHistoryAsync(string patientId);
    Task<MedicalRecordDto?> GetMedicalRecordWithLabResultsAsync(string recordId);
}