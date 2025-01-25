using AutoMapper;
using Medical.GrpcService.Context;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<PatientRepository> _logger;

    public PatientRepository(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<PatientRepository> logger) : base(context)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PatientDto?> GetPatientWithMedicalRecordsAsync(string patientId)
    {
        try
        {
            _logger.LogInformation("Getting patient with medical records: {PatientId}", patientId);
            var patient = await _context.Patients
                .Include(p => p.MedicalRecords)
                .ThenInclude(mr => mr.LabResults)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            return _mapper.Map<PatientDto>(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient with medical records {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<PatientDto>> GetPatientsByDoctorAsync(string doctorId)
    {
        try
        {
            _logger.LogInformation("Getting patients for doctor: {DoctorId}", doctorId);
            var patients = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.Patient)
                .Distinct()
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients for doctor {DoctorId}", doctorId);
            throw;
        }
    }
}