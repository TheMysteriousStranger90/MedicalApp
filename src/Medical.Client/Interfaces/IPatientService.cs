namespace Medical.Client.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientModel>> GetPatientsAsync(string doctorId);
    Task<PatientModel> GetPatientByIdAsync(string id);
    Task<GetMedicalRecordsResponse> GetPatientMedicalHistoryAsync(string patientId);
}