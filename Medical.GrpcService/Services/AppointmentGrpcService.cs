using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Medical.GrpcService.Services;

//[Authorize]
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
            _logger.LogInformation("Getting appointment with ID: {Id}", request.Id);

            if (!Guid.TryParse(request.Id, out _))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    "Invalid appointment ID format"));
            }

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
            if (appointment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Appointment not found with ID: {request.Id}"));
            }

            var response = _mapper.Map<AppointmentModel>(appointment);
            _logger.LogInformation("Successfully retrieved appointment {Id}", request.Id);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal,
                "Error retrieving appointment"));
        }
    }

    public override async Task<AppointmentModel> CreateAppointment(
        CreateAppointmentRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation(
                "Creating appointment with DoctorId: {DoctorId}, PatientId: {PatientId}, Date: {Date}",
                request.DoctorId, request.PatientId, request.AppointmentDate?.ToDateTime());

            if (string.IsNullOrEmpty(request.DoctorId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "DoctorId is required"));

            if (string.IsNullOrEmpty(request.PatientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "PatientId is required"));

            if (request.AppointmentDate == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AppointmentDate is required"));

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
            if (doctor == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Doctor not found"));

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                AppointmentDate = request.AppointmentDate.ToDateTime(),
                Notes = request.Notes ?? string.Empty,
                Symptoms = request.Symptoms ?? string.Empty,
                Fee = request.Fee,
                Status = AppointmentStatus.Scheduled,
                IsPaid = false
            };

            _logger.LogInformation("Saving appointment {AppointmentId} for patient {PatientId} with doctor {DoctorId}",
                appointment.Id, appointment.PatientId, appointment.DoctorId);

            await _unitOfWork.Appointments.AddAsync(appointment);
            var success = await _unitOfWork.Complete();

            if (!success)
            {
                _logger.LogError("Failed to save appointment to database");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create appointment"));
            }

            var response = _mapper.Map<AppointmentModel>(appointment);
            _logger.LogInformation("Successfully created appointment {AppointmentId}", appointment.Id);

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, $"Error creating appointment: {ex.Message}"));
        }
    }

    public override async Task<AppointmentModel> UpdateAppointment(UpdateAppointmentRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Updating appointment {Id} to status {Status}", request.Id, request.Status);

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
            if (appointment == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Appointment {request.Id} not found"));
            }

            var oldStatus = appointment.Status;

            appointment.Status = request.Status;
            appointment.Notes = request.Notes ?? appointment.Notes;
            appointment.Symptoms = request.Symptoms ?? appointment.Symptoms;
            appointment.IsPaid = request.IsPaid;
            appointment.Fee = request.Fee;

            // Create medical record only when completing appointment and if it doesn't already exist
            if (request.Status == AppointmentStatus.Completed && oldStatus != AppointmentStatus.Completed &&
                appointment.MedicalRecordId == null)
            {
                if (string.IsNullOrEmpty(request.Notes) || string.IsNullOrEmpty(request.Symptoms))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument,
                        "Notes (Diagnosis) and Symptoms (Treatment) are required to complete appointment"));
                }

                var medicalRecord = new MedicalRecord
                {
                    Id = Guid.NewGuid(),
                    PatientId = appointment.PatientId,
                    Diagnosis = request.Notes,
                    Treatment = request.Symptoms,
                    Notes = request.Notes,
                    Prescriptions = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);

                appointment.MedicalRecordId = medicalRecord.Id;

                _logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}",
                    medicalRecord.Id, appointment.Id);
            }

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            var success = await _unitOfWork.Complete();

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update appointment"));
            }

            return _mapper.Map<AppointmentModel>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, $"Error updating appointment: {ex.Message}"));
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