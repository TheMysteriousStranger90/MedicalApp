namespace Medical.Client.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentModel>> GetAppointmentsAsync(AppointmentRequest request);
    Task<AppointmentModel> GetAppointmentByIdAsync(string id);
    Task<AppointmentModel> CreateAppointmentAsync(CreateAppointmentRequest request);
    Task<AppointmentModel> UpdateAppointmentAsync(UpdateAppointmentRequest request);
    Task<DeleteAppointmentResponse> DeleteAppointmentAsync(string id);
}