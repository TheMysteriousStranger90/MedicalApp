using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Schedule;

public class IndexModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<IndexModel> _logger;

    public List<ScheduleModel> Schedules { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }
    [BindProperty]
    public string DeleteId { get; set; }

    public IndexModel(IDoctorService doctorService, ILogger<IndexModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }
            
            FromDate ??= DateTime.Today;
            ToDate ??= DateTime.Today.AddMonths(1);
            
            if (ToDate < FromDate)
            {
                ToDate = FromDate.Value.AddMonths(1);
            }

            _logger.LogInformation(
                "Getting schedules for doctor {DoctorId} from {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}",
                userId, FromDate, ToDate);

            var schedules = await _doctorService.GetDoctorScheduleAsync(
                userId,
                FromDate.Value,
                ToDate.Value);

            Schedules = schedules.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading schedules");
            ErrorMessage = "Failed to load schedules.";
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to delete schedule {ScheduleId}", DeleteId);

            if (string.IsNullOrEmpty(DeleteId))
            {
                _logger.LogWarning("DeleteId is null or empty");
                return BadRequest("Schedule ID is required");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var result = await _doctorService.DeleteScheduleAsync(DeleteId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Schedule deleted successfully";
                _logger.LogInformation("Schedule {ScheduleId} deleted successfully", DeleteId);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete schedule";
                _logger.LogWarning("Failed to delete schedule {ScheduleId}", DeleteId);
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", DeleteId);
            TempData["ErrorMessage"] = "Error deleting schedule";
            return RedirectToPage();
        }
    }
}