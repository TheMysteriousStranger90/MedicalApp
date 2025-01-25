using AutoMapper;
using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Repositories.Interfaces;

namespace Medical.GrpcService.Services;

public class MedicalRecordGrpcService : MedicalRecordService.MedicalRecordServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MedicalRecordGrpcService> _logger;

    public MedicalRecordGrpcService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MedicalRecordGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetMedicalRecordsResponse> GetMedicalRecords(
        GetMedicalRecordsRequest request,
        ServerCallContext context)
    {
        try
        {
            var records = await _unitOfWork.MedicalRecords
                .GetPatientMedicalHistoryAsync(request.PatientId);

            var response = new GetMedicalRecordsResponse();
            response.Records.AddRange(_mapper.Map<IEnumerable<MedicalRecordModel>>(records));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical records for patient {PatientId}", 
                request.PatientId);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error retrieving medical records"));
        }
    }

    public override async Task<MedicalRecordModel> CreateMedicalRecord(
        CreateMedicalRecordRequest request,
        ServerCallContext context)
    {
        try
        {
            var medicalRecord = new MedicalRecord
            {
                PatientId = request.PatientId,
                Diagnosis = request.Diagnosis,
                Treatment = request.Treatment,
                Prescriptions = request.Prescriptions,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                LabResults = _mapper.Map<List<LabResult>>(request.LabResults)
            };

            await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
            var success = await _unitOfWork.Complete();

            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, 
                    "Failed to create medical record"));
            }

            return _mapper.Map<MedicalRecordModel>(medicalRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medical record for patient {PatientId}", 
                request.PatientId);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Error creating medical record"));
        }
    }
}