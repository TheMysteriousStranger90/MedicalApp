using AutoMapper;
using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Repositories;

public class MedicalRecordRepository : GenericRepository<MedicalRecord>, IMedicalRecordRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<MedicalRecordRepository> _logger;

    public MedicalRecordRepository(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<MedicalRecordRepository> logger) : base(context)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetPatientMedicalHistoryAsync(string patientId)
    {
        try
        {
            _logger.LogInformation("Getting medical history for patient: {PatientId}", patientId);
            var records = await _context.MedicalRecords
                .Include(mr => mr.LabResults)
                .Where(mr => mr.PatientId == patientId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical history for patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<MedicalRecordDto?> GetMedicalRecordWithLabResultsAsync(string recordId)
    {
        try
        {
            _logger.LogInformation("Getting medical record with lab results: {RecordId}", recordId);
            var record = await _context.MedicalRecords
                .Include(mr => mr.LabResults)
                .Include(mr => mr.Patient)
                .FirstOrDefaultAsync(mr => mr.Id.ToString() == recordId);

            return _mapper.Map<MedicalRecordDto>(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical record with lab results {RecordId}", recordId);
            throw;
        }
    }
}