using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Doctors;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DetailsModel> _logger;

    public DoctorModel? Doctor { get; private set; }
    public string? ErrorMessage { get; set; }

    public DetailsModel(IDoctorService doctorService, ILogger<DetailsModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            Doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (Doctor == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor details for ID: {Id}", id);
            ErrorMessage = "Failed to load doctor details. Please try again later.";
            return Page();
        }
    }
}