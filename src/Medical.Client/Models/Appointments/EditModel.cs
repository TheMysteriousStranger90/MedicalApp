using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class EditModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<EditModel> _logger;
    [BindProperty] public UpdateAppointmentRequest Input { get; set; } = null!;
    public AppointmentModel Appointment { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public bool IsDoctor => User.IsInRole("Doctor");
    public bool IsPatient => User.IsInRole("Patient");

    public EditModel(
        IAppointmentService appointmentService,
        ILogger<EditModel> logger)
    {
        _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            Appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (Appointment == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                Appointment.DoctorId != userId &&
                Appointment.PatientId != userId)
            {
                return Forbid();
            }

            Input = new UpdateAppointmentRequest
            {
                Id = Appointment.Id,
                Status = Appointment.Status,
                Notes = Appointment.Notes,
                Symptoms = Appointment.Symptoms,
                IsPaid = Appointment.IsPaid
            };


            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load appointment");
            ErrorMessage = "Failed to load appointment.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var currentAppointment = await _appointmentService.GetAppointmentByIdAsync(Input.Id);

            if (currentAppointment == null)
                return NotFound();

            if (!User.IsInRole("Admin") &&
                currentAppointment.DoctorId != userId &&
                currentAppointment.PatientId != userId)
            {
                return Forbid();
            }

            if (User.IsInRole("Patient"))
            {
                Input.IsPaid = currentAppointment.IsPaid;
                Input.Fee = currentAppointment.Fee;
            }

            await _appointmentService.UpdateAppointmentAsync(Input);
            return RedirectToPage("./Details", new { id = Input.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update appointment");
            ErrorMessage = "Failed to update appointment.";
            return Page();
        }
    }
}