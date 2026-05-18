using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
{
    public Task<IEnumerable<MedicalRecordDto>> GetPatientMedicalHistoryAsync(string patientId);
    public Task<MedicalRecordDto?> GetMedicalRecordWithLabResultsAsync(string recordId);
}
