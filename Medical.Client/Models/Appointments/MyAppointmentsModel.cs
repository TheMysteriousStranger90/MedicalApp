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
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                ErrorMessage = "User not authenticated";
                return;
            }

            var request = new AppointmentRequest
            {
                Date = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            if (userRole == "Patient")
            {
                _logger.LogInformation("Fetching appointments for patient {PatientId}", userId);
                request.PatientId = userId;
            }
            else if (userRole == "Doctor")
            {
                _logger.LogInformation("Fetching appointments for doctor {DoctorId}", userId);
                request.DoctorId = userId;
            }
            else
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId}", userId);
                ErrorMessage = "Unauthorized access";
                return;
            }

            Appointments = await _appointmentService.GetAppointmentsAsync(request);
            _logger.LogInformation("Retrieved {Count} appointments for {Role} {UserId}", 
                Appointments.Count(), userRole, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments");
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
            
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment.PatientId != userId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt - AppointmentId: {Id}, UserId: {UserId}", 
                    id, userId);
                return Unauthorized();
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