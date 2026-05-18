using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.MedicalRecords;

[Authorize]
public class HistoryModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<HistoryModel> _logger;

    public IEnumerable<MedicalRecordModel> MedicalRecords { get; private set; } = new List<MedicalRecordModel>();
    public PatientModel? Patient { get; private set; }
    public string? ErrorMessage { get; set; }

    public HistoryModel(IPatientService patientService, ILogger<HistoryModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string patientId)
    {
        try
        {
            var historyResponse = await _patientService.GetPatientMedicalHistoryAsync(patientId);
            MedicalRecords = historyResponse.Records;
            Patient = await _patientService.GetPatientByIdAsync(patientId);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical history for patient ID: {PatientId}", patientId);
            ErrorMessage = "Failed to load medical history. Please try again later.";
            return Page();
        }
    }
}