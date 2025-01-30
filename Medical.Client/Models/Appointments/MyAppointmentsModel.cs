using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class MyAppointmentsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<MyAppointmentsModel> _logger;

    public IEnumerable<AppointmentViewModel> Appointments { get; private set; } = new List<AppointmentViewModel>();
    public string? ErrorMessage { get; set; }

    public MyAppointmentsModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<MyAppointmentsModel> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
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

            // Role-based filtering
            if (userRole == "Patient")
            {
                request.PatientId = userId;
                var appointments = await _appointmentService.GetAppointmentsAsync(request);
                var viewModels = new List<AppointmentViewModel>();

                foreach (var appointment in appointments.Where(a => a.PatientId == userId)) // Additional security check
                {
                    var doctor = await _doctorService.GetDoctorByIdAsync(appointment.DoctorId);
                    viewModels.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate.ToDateTime().ToLocalTime(),
                        DoctorName = doctor?.FullName ?? "Unknown",
                        PatientName = "You",
                        Status = appointment.Status
                    });
                }

                Appointments = viewModels;
            }
            else if (userRole == "Doctor")
            {
                request.DoctorId = userId;
                var appointments = await _appointmentService.GetAppointmentsAsync(request);
                var viewModels = new List<AppointmentViewModel>();

                foreach (var appointment in appointments.Where(a => a.DoctorId == userId))
                {
                    var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
                    viewModels.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate.ToDateTime().ToLocalTime(),
                        DoctorName = "You",
                        PatientName = patient?.FullName ?? "Unknown",
                        Status = appointment.Status
                    });
                }

                Appointments = viewModels;
            }
            else
            {
                ErrorMessage = "Unauthorized access";
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments for user {UserId}",
                User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
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