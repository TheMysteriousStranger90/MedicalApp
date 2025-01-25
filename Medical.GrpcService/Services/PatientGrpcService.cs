using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Repositories.Interfaces;

namespace Medical.GrpcService.Services;

public class PatientGrpcService : PatientService.PatientServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientGrpcService> _logger;

    public PatientGrpcService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PatientGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetPatientsResponse> GetPatients(
        GetPatientsRequest request, 
        ServerCallContext context)
    {
        try
        {
            var patients = await _unitOfWork.Patients.GetPatientsByDoctorAsync(request.DoctorId);
            var response = new GetPatientsResponse();
            response.Patients.AddRange(_mapper.Map<IEnumerable<PatientModel>>(patients));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients for doctor {DoctorId}", request.DoctorId);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving patients"));
        }
    }

    public override async Task<PatientModel> GetPatientById(
        GetPatientByIdRequest request, 
        ServerCallContext context)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetPatientWithMedicalRecordsAsync(request.Id);
            if (patient == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Patient with ID {request.Id} not found"));
            }

            return _mapper.Map<PatientModel>(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by id {PatientId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving patient"));
        }
    }

    public override async Task<GetMedicalRecordsResponse> GetPatientMedicalHistory(
        GetPatientByIdRequest request, 
        ServerCallContext context)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetPatientWithMedicalRecordsAsync(request.Id);
            if (patient == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Patient with ID {request.Id} not found"));
            }

            var response = new GetMedicalRecordsResponse();
            if (patient.MedicalRecords != null)
            {
                response.Records.AddRange(_mapper.Map<IEnumerable<MedicalRecordModel>>(patient.MedicalRecords));
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical history for patient {PatientId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error retrieving patient medical history"));
        }
    }
}