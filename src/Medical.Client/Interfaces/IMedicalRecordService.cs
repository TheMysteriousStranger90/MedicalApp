namespace Medical.Client.Interfaces;

public interface IMedicalRecordService
{
    public Task<IEnumerable<MedicalRecordModel>> GetMedicalRecordsAsync(string patientId);
    public Task<MedicalRecordModel> CreateMedicalRecordAsync(CreateMedicalRecordRequest request);
}
