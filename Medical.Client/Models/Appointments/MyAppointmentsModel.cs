using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
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
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                ErrorMessage = "User not authenticated";
                return;
            }

            _logger.LogInformation("Fetching appointments for patient {PatientId}", userId);

            var request = new AppointmentRequest
            {
                PatientId = userId,
                Date = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            Appointments = await _appointmentService.GetAppointmentsAsync(request);
            _logger.LogInformation("Retrieved {Count} appointments", Appointments.Count());
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
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Invalid appointment ID for cancellation");
                ErrorMessage = "Invalid appointment ID";
                return Page();
            }

            _logger.LogInformation("Cancelling appointment {Id}", id);

            var request = new UpdateAppointmentRequest
            {
                Id = id,
                Status = AppointmentStatus.Cancelled,
                Notes = "Cancelled by patient"
            };

            await _appointmentService.UpdateAppointmentAsync(request);
            _logger.LogInformation("Successfully cancelled appointment {Id}", id);

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {Id}", id);
            ErrorMessage = "Failed to cancel appointment.";
            await OnGetAsync();
            return Page();
        }
    }
}