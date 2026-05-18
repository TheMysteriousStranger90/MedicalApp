using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Patients;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<DetailsModel> _logger;

    public PatientModel? Patient { get; private set; }
    public string? ErrorMessage { get; set; }

    public DetailsModel(IPatientService patientService, ILogger<DetailsModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            Patient = await _patientService.GetPatientByIdAsync(id);
            if (Patient == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient details for ID: {Id}", id);
            ErrorMessage = "Failed to load patient details. Please try again later.";
            return Page();
        }
    }
}