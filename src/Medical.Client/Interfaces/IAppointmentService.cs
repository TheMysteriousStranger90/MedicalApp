namespace Medical.Client.Interfaces;

public interface IAppointmentService
{
    public Task<IEnumerable<AppointmentModel>> GetAppointmentsAsync(AppointmentRequest request);
    public Task<AppointmentModel> GetAppointmentByIdAsync(string id);
    public Task<AppointmentModel> CreateAppointmentAsync(CreateAppointmentRequest request);
    public Task<AppointmentModel> UpdateAppointmentAsync(UpdateAppointmentRequest request);
    public Task<DeleteAppointmentResponse> DeleteAppointmentAsync(string id);
}
