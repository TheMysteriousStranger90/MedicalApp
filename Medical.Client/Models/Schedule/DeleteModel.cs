using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Schedule;

[Authorize(Roles = "Doctor")]
public class DeleteModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DeleteModel> _logger;

    [BindProperty]
    public string ScheduleId { get; set; }

    public DeleteModel(IDoctorService doctorService, ILogger<DeleteModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public IActionResult OnGet(string id)
    {
        ScheduleId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await _doctorService.DeleteScheduleAsync(ScheduleId);
            if (result.Success)
            {
                TempData["Message"] = "Schedule deleted successfully";
                return RedirectToPage("./Index");
            }
            
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", ScheduleId);
            TempData["Error"] = "Failed to delete schedule";
            return RedirectToPage("./Index");
        }
    }
}