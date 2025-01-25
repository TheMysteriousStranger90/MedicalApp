namespace Medical.GrpcService.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    IDoctorRepository Doctors { get; }
    IPatientRepository Patients { get; }
    IMedicalRecordRepository MedicalRecords { get; }
    Task<bool> Complete();
}