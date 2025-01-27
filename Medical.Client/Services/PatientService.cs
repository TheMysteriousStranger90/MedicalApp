using Grpc.Core;
using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class PatientServiceGrpc : IPatientService
{
    private readonly  Medical.Client.PatientService.PatientServiceClient _client;
    private readonly ILogger<PatientServiceGrpc> _logger;

    public PatientServiceGrpc(
        Medical.Client.PatientService.PatientServiceClient client,
        ILogger<PatientServiceGrpc> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<PatientModel>> GetPatientsAsync(string doctorId)
    {
        try
        {
            var request = new GetPatientsRequest { DoctorId = doctorId };
            var response = await _client.GetPatientsAsync(request);
            return response.Patients;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting patients for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<PatientModel> GetPatientByIdAsync(string id)
    {
        try
        {
            var request = new GetPatientByIdRequest { Id = id };
            return await _client.GetPatientByIdAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting patient by id {PatientId}", id);
            throw;
        }
    }

    public async Task<GetMedicalRecordsResponse> GetPatientMedicalHistoryAsync(string patientId)
    {
        try
        {
            var request = new GetPatientByIdRequest { Id = patientId };
            return await _client.GetPatientMedicalHistoryAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting medical history for patient {PatientId}", patientId);
            throw;
        }
    }
}