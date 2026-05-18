using Grpc.Core;
using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class MedicalRecordServiceGrpc : IMedicalRecordService
{
    private readonly  Medical.Client.MedicalRecordService.MedicalRecordServiceClient _client;
    private readonly ILogger<MedicalRecordServiceGrpc> _logger;

    public MedicalRecordServiceGrpc(
        Medical.Client.MedicalRecordService.MedicalRecordServiceClient client,
        ILogger<MedicalRecordServiceGrpc> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<MedicalRecordModel>> GetMedicalRecordsAsync(string patientId)
    {
        try
        {
            var request = new GetMedicalRecordsRequest { PatientId = patientId };
            var response = await _client.GetMedicalRecordsAsync(request);
            return response.Records;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting medical records for patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<MedicalRecordModel> CreateMedicalRecordAsync(CreateMedicalRecordRequest request)
    {
        try
        {
            return await _client.CreateMedicalRecordAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error creating medical record for patient {PatientId}", request.PatientId);
            throw;
        }
    }
}