using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class DoctorServiceGrpc : IDoctorService
{
    private readonly Medical.Client.DoctorService.DoctorServiceClient _client;
    private readonly ILogger<DoctorServiceGrpc> _logger;

    public DoctorServiceGrpc(
        Medical.Client.DoctorService.DoctorServiceClient client,
        ILogger<DoctorServiceGrpc> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<DoctorModel>> GetAllDoctorsAsync()
    {
        try
        {
            var request = new GetDoctorsRequest { Specialization = string.Empty };
            var response = await _client.GetDoctorsAsync(request);
            return response.Doctors;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting all doctors");
            throw;
        }
    }

    public async Task<IEnumerable<DoctorModel>> GetDoctorsBySpecializationAsync(string specialization)
    {
        try
        {
            if (string.IsNullOrEmpty(specialization))
                return await GetAllDoctorsAsync();

            var request = new GetDoctorsRequest { Specialization = specialization };
            var response = await _client.GetDoctorsAsync(request);
            return response.Doctors;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting doctors by specialization {Specialization}", specialization);
            throw;
        }
    }

    public async Task<DoctorModel> GetDoctorByIdAsync(string id)
    {
        try
        {
            var request = new GetDoctorByIdRequest { Id = id };
            return await _client.GetDoctorByIdAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting doctor by id {DoctorId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DoctorModel>> GetAvailableDoctorsAsync(DateTime date)
    {
        try
        {
            var request = new GetAvailableDoctorsRequest 
            { 
                Date = Timestamp.FromDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc)) 
            };
            var response = await _client.GetAvailableDoctorsAsync(request);
            return response.Doctors;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting available doctors for date {Date}", date);
            throw;
        }
    }
}