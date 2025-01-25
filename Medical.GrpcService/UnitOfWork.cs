using AutoMapper;
using Medical.GrpcService.Context;
using Medical.GrpcService.Repositories;
using Medical.GrpcService.Repositories.Interfaces;

namespace Medical.GrpcService;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILoggerFactory _loggerFactory;
    private IAppointmentRepository _appointmentRepository;
    private IDoctorRepository _doctorRepository;
    private IPatientRepository _patientRepository;
    private IMedicalRecordRepository _medicalRecordRepository;

    public UnitOfWork(
        ApplicationDbContext context,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _mapper = mapper;
        _loggerFactory = loggerFactory;
    }

    public IAppointmentRepository Appointments => 
        _appointmentRepository ??= new AppointmentRepository(
            _context,
            _mapper,
            _loggerFactory.CreateLogger<AppointmentRepository>());

    public IDoctorRepository Doctors => 
        _doctorRepository ??= new DoctorRepository(
            _context,
            _mapper,
            _loggerFactory.CreateLogger<DoctorRepository>());

    public IPatientRepository Patients => 
        _patientRepository ??= new PatientRepository(
            _context,
            _mapper,
            _loggerFactory.CreateLogger<PatientRepository>());

    public IMedicalRecordRepository MedicalRecords => 
        _medicalRecordRepository ??= new MedicalRecordRepository(
            _context,
            _mapper,
            _loggerFactory.CreateLogger<MedicalRecordRepository>());

    public async Task<bool> Complete()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}