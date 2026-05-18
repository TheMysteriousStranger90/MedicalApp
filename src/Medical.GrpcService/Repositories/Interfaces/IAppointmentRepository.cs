using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;

namespace Medical.GrpcService.Repositories.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(string doctorId);
    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date);
    Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync();
}