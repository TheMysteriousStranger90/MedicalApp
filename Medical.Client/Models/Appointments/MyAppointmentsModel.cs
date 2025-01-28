using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class MyAppointmentsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<MyAppointmentsModel> _logger;

    public IEnumerable<AppointmentModel> Appointments { get; private set; } = new List<AppointmentModel>();
    public string? ErrorMessage { get; set; }

    public MyAppointmentsModel(IAppointmentService appointmentService, ILogger<MyAppointmentsModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var patientId = User.Claims.FirstOrDefault(c => c.Type == "PatientId")?.Value;
            if (string.IsNullOrEmpty(patientId))
                return;

            var request = new AppointmentRequest { PatientId = patientId };
            Appointments = await _appointmentService.GetAppointmentsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading patient appointments");
            ErrorMessage = "Failed to load appointments.";
        }
    }

    public async Task<IActionResult> OnPostCancelAsync(string id)
    {
        try
        {
            var request = new UpdateAppointmentRequest
            {
                Id = id,
                Status = AppointmentStatus.Cancelled
            };
            await _appointmentService.UpdateAppointmentAsync(request);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment");
            ErrorMessage = "Failed to cancel appointment.";
            return Page();
        }
    }
}