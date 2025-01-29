using Grpc.Core;
using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class AppointmentServiceGrpc : IAppointmentService
{
    private readonly AppointmentService.AppointmentServiceClient _client;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<AppointmentServiceGrpc> _logger;

    public AppointmentServiceGrpc(
        AppointmentService.AppointmentServiceClient client,
        ITokenStorageService tokenStorage,
        ILogger<AppointmentServiceGrpc> logger)
    {
        _client = client;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task<IEnumerable<AppointmentModel>> GetAppointmentsAsync(AppointmentRequest request)
    {
        try
        {
            _logger.LogInformation("Requesting appointments for PatientId: {PatientId}", request.PatientId);
        
            var metadata = new Metadata
            {
                { "Authorization", $"Bearer {_tokenStorage.GetToken()}" }
            };

            var response = await _client.GetAppointmentsAsync(request, metadata);
            _logger.LogInformation("Received {Count} appointments", response.Appointments.Count);
        
            return response.Appointments;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting appointments. Status: {Status}", ex.Status);
            throw;
        }
    }

    public async Task<AppointmentModel> GetAppointmentByIdAsync(string id)
    {
        try
        {
            var request = new AppointmentByIdRequest { Id = id };
            var metadata = new Metadata
            {
                { "Authorization", $"Bearer {_tokenStorage.GetToken()}" }
            };
        
            return await _client.GetAppointmentByIdAsync(request, metadata);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting appointment by id {Id}", id);
            throw;
        }
    }

    public async Task<AppointmentModel> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        try
        {
            var token = _tokenStorage.GetToken();
            _logger.LogInformation("Creating appointment with token: {TokenExists}", !string.IsNullOrEmpty(token));
            
            return await _client.CreateAppointmentAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error creating appointment. Status: {Status}, Detail: {Detail}", 
                ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    public async Task<AppointmentModel> UpdateAppointmentAsync(UpdateAppointmentRequest request)
    {
        try
        {
            var metadata = new Metadata
            {
                { "Authorization", $"Bearer {_tokenStorage.GetToken()}" }
            };

            _logger.LogInformation("Updating appointment {Id} with status {Status}", 
                request.Id, request.Status);

            var response = await _client.UpdateAppointmentAsync(request, metadata);
            _logger.LogInformation("Successfully updated appointment {Id}", request.Id);
            
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}. Status: {Status}, Detail: {Detail}", 
                request.Id, ex.StatusCode, ex.Status.Detail);
            throw;
        }
    }

    public async Task<DeleteAppointmentResponse> DeleteAppointmentAsync(string id)
    {
        try
        {
            var request = new DeleteAppointmentRequest { Id = id };
            return await _client.DeleteAppointmentAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error deleting appointment {Id}", id);
            throw;
        }
    }
}