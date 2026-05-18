namespace Medical.GrpcService.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    public IAppointmentRepository Appointments { get; }
    public IDoctorRepository Doctors { get; }
    public IPatientRepository Patients { get; }
    public IMedicalRecordRepository MedicalRecords { get; }
    public Task<bool> Complete();
}
