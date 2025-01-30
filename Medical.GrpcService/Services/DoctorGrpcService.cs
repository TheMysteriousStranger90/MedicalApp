using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Repositories.Interfaces;

namespace Medical.GrpcService.Services;

public class DoctorGrpcService : DoctorService.DoctorServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorGrpcService> _logger;

    public DoctorGrpcService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DoctorGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetDoctorsResponse> GetDoctors(GetDoctorsRequest request, ServerCallContext context)
    {
        try
        {
            var doctors = string.IsNullOrEmpty(request.Specialization)
                ? await _unitOfWork.Doctors.GetAllDoctorsAsync()
                : await _unitOfWork.Doctors.GetDoctorsBySpecializationAsync(request.Specialization);

            var response = new GetDoctorsResponse();
            response.Doctors.AddRange(_mapper.Map<IEnumerable<DoctorModel>>(doctors));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors");
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving doctors"));
        }
    }

    public override async Task<DoctorModel> GetDoctorById(
        GetDoctorByIdRequest request,
        ServerCallContext context)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetDoctorWithSchedulesAsync(request.Id);
            if (doctor == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Doctor with ID {request.Id} not found"));
            }

            return _mapper.Map<DoctorModel>(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor by id {DoctorId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal,
                "Error retrieving doctor"));
        }
    }

    public override async Task<GetDoctorsResponse> GetAvailableDoctors(
        GetAvailableDoctorsRequest request,
        ServerCallContext context)
    {
        try
        {
            var date = request.Date.ToDateTime();
            var doctors = await _unitOfWork.Doctors.GetAvailableDoctorsAsync(date);
            var response = new GetDoctorsResponse();
            response.Doctors.AddRange(_mapper.Map<IEnumerable<DoctorModel>>(doctors));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available doctors for date {Date}",
                request.Date.ToDateTime());
            throw new RpcException(new Status(StatusCode.Internal,
                "Error retrieving available doctors"));
        }
    }

    public override async Task<ScheduleModel> UpdateSchedule(UpdateScheduleRequest request, ServerCallContext context)
    {
        try
        {
            var schedule = await _unitOfWork.Doctors.GetScheduleByIdAsync(request.Id);
            if (schedule == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Schedule not found"));
            
            schedule.DayOfWeek = (DayOfWeek)request.DayOfWeek;
            schedule.StartTime = request.StartTime.ToDateTime().TimeOfDay;
            schedule.EndTime = request.EndTime.ToDateTime().TimeOfDay;
            schedule.SlotDurationMinutes = request.SlotDurationMinutes;
            schedule.IsAvailable = request.IsAvailable;
            schedule.Notes = request.Notes;

            _logger.LogInformation(
                "Processing update for schedule {Id} - Day: {Day}, Duration: {Duration}min", 
                request.Id, request.DayOfWeek, request.SlotDurationMinutes);

            var result = await _unitOfWork.Doctors.UpdateScheduleAsync(schedule);
            return _mapper.Map<ScheduleModel>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update schedule {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<DeleteScheduleResponse> DeleteSchedule(DeleteScheduleRequest request,
        ServerCallContext context)
    {
        try
        {
            var success = await _unitOfWork.Doctors.DeleteScheduleAsync(request.Id);
            return new DeleteScheduleResponse
            {
                Success = success,
                Message = success ? "Schedule deleted successfully" : "Schedule not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Error deleting schedule"));
        }
    }

    public override async Task<GetDoctorScheduleResponse> GetDoctorSchedule(GetDoctorScheduleRequest request,
        ServerCallContext context)
    {
        try
        {
            var schedules = await _unitOfWork.Doctors.GetDoctorSchedulesAsync(
                request.DoctorId,
                request.FromDate.ToDateTime(),
                request.ToDate.ToDateTime());

            var response = new GetDoctorScheduleResponse();
            response.Schedules.AddRange(_mapper.Map<IEnumerable<ScheduleModel>>(schedules));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedules for doctor {DoctorId}", request.DoctorId);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving doctor schedules"));
        }
    }

    public override async Task<ScheduleModel> CreateSchedule(CreateScheduleRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation(
                "Creating schedule request - Doctor: {DoctorId}, Day: {Day}, Start: {Start}, End: {End}",
                request.DoctorId,
                request.DayOfWeek,
                request.StartTime.ToDateTime(),
                request.EndTime.ToDateTime());

            if (request.EndTime.ToDateTime() <= request.StartTime.ToDateTime())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "End time must be after start time"));
            }

            var schedule = new Schedule
            {
                DoctorId = request.DoctorId,
                DayOfWeek = (DayOfWeek)request.DayOfWeek,
                StartTime = TimeSpan.FromTicks(request.StartTime.ToDateTime().TimeOfDay.Ticks),
                EndTime = TimeSpan.FromTicks(request.EndTime.ToDateTime().TimeOfDay.Ticks),
                SlotDurationMinutes = request.SlotDurationMinutes,
                ValidFrom = request.ValidFrom?.ToDateTime(),
                ValidTo = request.ValidTo?.ToDateTime(),
                Notes = request.Notes,
                IsAvailable = true
            };

            _logger.LogInformation(
                "Schedule object created - StartTime: {Start}, EndTime: {End}, ValidFrom: {ValidFrom}, ValidTo: {ValidTo}",
                schedule.StartTime,
                schedule.EndTime,
                schedule.ValidFrom,
                schedule.ValidTo);

            var result = await _unitOfWork.Doctors.CreateScheduleAsync(schedule);
            return _mapper.Map<ScheduleModel>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule for doctor {DoctorId}", request.DoctorId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}