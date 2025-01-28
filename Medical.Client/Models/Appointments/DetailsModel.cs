using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class DetailsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly ILogger<DetailsModel> _logger;

    public AppointmentModel? Appointment { get; private set; }
    public string? ErrorMessage { get; set; }

    public DetailsModel(IAppointmentService appointmentService, ILogger<DetailsModel> logger)
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

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointment details {Id}", id);
            ErrorMessage = "Failed to load appointment details.";
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostCompleteAsync(string id)
    {
        try
        {
            var updateRequest = new UpdateAppointmentRequest
            {
                Id = id,
                Status = AppointmentStatus.Completed,
                Notes = Request.Form["Diagnosis"].ToString(),
                Symptoms = Request.Form["Treatment"].ToString()
            };
            
            await _appointmentService.UpdateAppointmentAsync(updateRequest);
            
            var createRecordRequest = new CreateMedicalRecordRequest
            {
                PatientId = Appointment.PatientId,
                Diagnosis = Request.Form["Diagnosis"],
                Treatment = Request.Form["Treatment"],
                Prescriptions = Request.Form["Prescriptions"],
                Notes = Request.Form["Notes"]
            };
            
            await _medicalRecordService.CreateMedicalRecordAsync(createRecordRequest);

            return RedirectToPage("/MedicalRecords/History", new { patientId = Appointment.PatientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment");
            ErrorMessage = "Failed to complete appointment";
            return Page();
        }
    }
}