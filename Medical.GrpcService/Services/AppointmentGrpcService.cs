using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Repositories.Interfaces;

namespace Medical.GrpcService.Services;

public class AppointmentGrpcService : AppointmentService.AppointmentServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentGrpcService> _logger;

    public AppointmentGrpcService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AppointmentGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<AppointmentResponse> GetAppointments(
        AppointmentRequest request, 
        ServerCallContext context)
    {
        try
        {
            var appointments = await _unitOfWork.Appointments.GetAllAsync();
            var response = new AppointmentResponse();
            response.Appointments.AddRange(
                _mapper.Map<IEnumerable<AppointmentModel>>(appointments));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointments");
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error getting appointments"));
        }
    }

    public override async Task<AppointmentModel> GetAppointmentById(
        AppointmentByIdRequest request, 
        ServerCallContext context)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
            if (appointment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    "Appointment not found"));
            }
            return _mapper.Map<AppointmentModel>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment by id {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error getting appointment"));
        }
    }

    public override async Task<AppointmentModel> CreateAppointment(
        CreateAppointmentRequest request, 
        ServerCallContext context)
    {
        try
        {
            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                AppointmentDate = request.AppointmentDate.ToDateTime(),
                Notes = request.Notes,
                Symptoms = request.Symptoms,
                Fee = request.Fee,
                Status = AppointmentStatus.Scheduled,
                IsPaid = false
            };

            await _unitOfWork.Appointments.AddAsync(appointment);
            var success = await _unitOfWork.Complete();
            
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, 
                    "Failed to create appointment"));
            }

            return _mapper.Map<AppointmentModel>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error creating appointment"));
        }
    }

    public override async Task<AppointmentModel> UpdateAppointment(
        UpdateAppointmentRequest request, 
        ServerCallContext context)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
            if (appointment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    "Appointment not found"));
            }

            appointment.Status = request.Status;
            appointment.Notes = request.Notes;
            appointment.Symptoms = request.Symptoms;
            appointment.IsPaid = request.IsPaid;

            await _unitOfWork.Complete();

            return _mapper.Map<AppointmentModel>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error updating appointment"));
        }
    }

    public override async Task<DeleteAppointmentResponse> DeleteAppointment(
        DeleteAppointmentRequest request, 
        ServerCallContext context)
    {
        try
        {
            var result = await _unitOfWork.Appointments.DeleteAsync(request.Id);
            await _unitOfWork.Complete();

            return new DeleteAppointmentResponse
            {
                Success = result,
                Message = result ? "Appointment deleted successfully" : "Appointment not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error deleting appointment"));
        }
    }
}
     