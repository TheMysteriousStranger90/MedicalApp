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
    
        public async Task<ScheduleModel> CreateScheduleAsync(CreateScheduleRequest request)
    {
        try
        {
            var response = await _client.CreateScheduleAsync(request);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error creating schedule for doctor {DoctorId}", request.DoctorId);
            throw;
        }
    }

    public async Task<ScheduleModel> UpdateScheduleAsync(UpdateScheduleRequest request)
    {
        try
        {
            var response = await _client.UpdateScheduleAsync(request);
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId}", request.Id);
            throw;
        }
    }

    public async Task<DeleteScheduleResponse> DeleteScheduleAsync(string id)
    {
        try
        {
            var request = new DeleteScheduleRequest { Id = id };
            return await _client.DeleteScheduleAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ScheduleModel>> GetDoctorScheduleAsync(string doctorId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var request = new GetDoctorScheduleRequest
            {
                DoctorId = doctorId,
                FromDate = Timestamp.FromDateTime(DateTime.SpecifyKind(fromDate, DateTimeKind.Utc)),
                ToDate = Timestamp.FromDateTime(DateTime.SpecifyKind(toDate, DateTimeKind.Utc))
            };

            var response = await _client.GetDoctorScheduleAsync(request);
            return response.Schedules;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting schedule for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<IEnumerable<TimeSlotModel>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date)
    {
        try
        {
            var schedules = await GetDoctorScheduleAsync(doctorId, date.Date, date.Date.AddDays(1));
            return schedules
                .SelectMany(s => s.TimeSlots)
                .Where(ts => !ts.IsBooked)
                .OrderBy(ts => ts.StartTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available time slots for doctor {DoctorId}", doctorId);
            throw;
        }
    }
}