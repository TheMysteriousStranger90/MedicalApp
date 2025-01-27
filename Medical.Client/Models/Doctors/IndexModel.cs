using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Doctors;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<DoctorModel> Doctors { get; private set; } = new List<DoctorModel>();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)] public string? Specialization { get; set; }

    public IndexModel(IDoctorService doctorService, ILogger<IndexModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Doctors = await _doctorService.GetDoctorsAsync(Specialization ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors");
            ErrorMessage = "Failed to load doctors. Please try again later.";
        }
    }
}