using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Medical.Client.Helpers;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Schedule;

[Authorize(Roles = "Doctor")]
public class CreateModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<CreateModel> _logger;

    [BindProperty] public CreateScheduleRequest Input { get; set; } = new();

    [BindProperty] public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [BindProperty] public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

    [BindProperty] public DateTime ValidFrom { get; set; } = DateTime.Today;

    [BindProperty] public DateTime ValidTo { get; set; } = DateTime.Today.AddMonths(3);

    public string? ErrorMessage { get; set; }

    public CreateModel(IDoctorService doctorService, ILogger<CreateModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public List<ScheduleModel> ExistingSchedules { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Input.DoctorId = userId;

        // Load existing schedules
        ExistingSchedules = (await _doctorService.GetDoctorScheduleAsync(
            userId,
            DateTime.Today,
            DateTime.Today.AddMonths(3))).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Doctor") || string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (EndTime <= StartTime)
            {
                ModelState.AddModelError(string.Empty, "End time must be after start time");
                return Page();
            }

            if (ValidTo <= ValidFrom)
            {
                ModelState.AddModelError(string.Empty, "Valid to date must be after valid from date");
                return Page();
            }

            _logger.LogInformation(
                "Creating schedule - DayOfWeek: {Day}, StartTime: {Start}, EndTime: {End}",
                Input.DayOfWeek, StartTime, EndTime);

            // Store TimeSpan values directly without timezone conversion
            var baseDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
            Input.StartTime = Timestamp.FromDateTime(baseDate.Add(StartTime));
            Input.EndTime = Timestamp.FromDateTime(baseDate.Add(EndTime));

            // Only convert dates to UTC
            Input.ValidFrom = Timestamp.FromDateTime(ValidFrom.Date.ToUtcTime());
            Input.ValidTo = Timestamp.FromDateTime(ValidTo.Date.ToUtcTime());
            Input.DoctorId = userId;
            Input.SlotDurationMinutes = Input.SlotDurationMinutes == 0 ? 30 : Input.SlotDurationMinutes;

            var result = await _doctorService.CreateScheduleAsync(Input);
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            ErrorMessage = "Failed to create schedule. Please try again.";
            await OnGetAsync();
            return Page();
        }
    }
}