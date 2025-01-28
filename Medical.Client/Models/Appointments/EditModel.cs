using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class EditModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<EditModel> _logger;

    [BindProperty] public UpdateAppointmentRequest Input { get; set; }
    public AppointmentModel Appointment { get; set; }
    public string? ErrorMessage { get; set; }

    public EditModel(IAppointmentService appointmentService, ILogger<EditModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            Appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (Appointment == null)
                return NotFound();

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
            _logger.LogError(ex, "Error loading appointment {Id}", id);
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

            await _appointmentService.UpdateAppointmentAsync(Input);
            return RedirectToPage("./Details", new { id = Input.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", Input.Id);
            ErrorMessage = "Failed to update appointment.";
            return Page();
        }
    }
}