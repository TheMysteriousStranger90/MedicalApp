using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    public Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(string doctorId);
    public Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientId);
    public Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date);
    public Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync();
}
