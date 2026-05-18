namespace Medical.Client.Interfaces;

public interface IPatientService
{
    public Task<IEnumerable<PatientModel>> GetPatientsAsync(string doctorId);
    public Task<PatientModel> GetPatientByIdAsync(string id);
    public Task<GetMedicalRecordsResponse> GetPatientMedicalHistoryAsync(string patientId);
}
