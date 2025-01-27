using Medical.Client.Attributes;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Patients;

[DoctorRoleOnly]
public class MyPatientsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<MyPatientsModel> _logger;

    public IEnumerable<PatientModel> Patients { get; private set; } = new List<PatientModel>();
    public string? ErrorMessage { get; set; }

    public MyPatientsModel(IPatientService patientService, ILogger<MyPatientsModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var doctorId = User.Claims.FirstOrDefault(c => c.Type == "DoctorId")?.Value;
            if (!string.IsNullOrEmpty(doctorId))
            {
                Patients = await _patientService.GetPatientsAsync(doctorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            ErrorMessage = "Failed to load patients. Please try again later.";
        }
    }
}