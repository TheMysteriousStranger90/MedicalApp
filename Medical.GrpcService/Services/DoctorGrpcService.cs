using AutoMapper;
using Grpc.Core;
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
}