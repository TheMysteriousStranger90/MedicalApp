namespace Medical.Client.Interfaces;

public interface IMedicalRecordService
{
    Task<IEnumerable<MedicalRecordModel>> GetMedicalRecordsAsync(string patientId);
    Task<MedicalRecordModel> CreateMedicalRecordAsync(CreateMedicalRecordRequest request);
}