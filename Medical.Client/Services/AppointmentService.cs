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
            var response = await _client.GetAppointmentsAsync(request);
            return response.Appointments;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting appointments");
            throw;
        }
    }

    public async Task<AppointmentModel> GetAppointmentByIdAsync(string id)
    {
        try
        {
            var request = new AppointmentByIdRequest { Id = id };
            return await _client.GetAppointmentByIdAsync(request);
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
            var metadata = new Metadata
            {
                { "Authorization", $"Bearer {_tokenStorage.GetToken()}" }
            };

            return await _client.CreateAppointmentAsync(request, metadata);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            throw;
        }
    }

    public async Task<AppointmentModel> UpdateAppointmentAsync(UpdateAppointmentRequest request)
    {
        try
        {
            return await _client.UpdateAppointmentAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", request.Id);
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