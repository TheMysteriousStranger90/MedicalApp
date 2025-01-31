using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Medical.Client.Helpers;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class CreateModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly ILogger<CreateModel> _logger;
    private readonly ITokenStorageService _tokenStorage;

    [BindProperty]
    public CreateAppointmentRequest Input { get; set; } = new();

    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime AppointmentDateTime { get; set; } = DateTime.Now
        .AddHours(1)
        .AddSeconds(-DateTime.Now.Second)
        .AddMilliseconds(-DateTime.Now.Millisecond);
    public List<DoctorModel> AvailableDoctors { get; set; } = new();
    public DoctorModel? SelectedDoctor { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ScheduleModel> DoctorSchedules { get; set; } = new();
    public List<TimeSlotModel> AvailableTimeSlots { get; set; } = new();
    

    public CreateModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        ITokenStorageService tokenStorage,
        ILogger<CreateModel> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task OnGetAsync(string? doctorId)
    {
        try
        {
            if (!string.IsNullOrEmpty(doctorId))
            {
                SelectedDoctor = await _doctorService.GetDoctorByIdAsync(doctorId);
                Input.DoctorId = doctorId;

                // Get schedule for next month
                var schedules = await _doctorService.GetDoctorScheduleAsync(
                    doctorId,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMonths(1));

                DoctorSchedules = schedules
                    .Where(s => s.IsAvailable)
                    .ToList();

                // Get available slots
                AvailableTimeSlots = DoctorSchedules
                    .SelectMany(s => s.TimeSlots)
                    .Where(ts => !ts.IsBooked)
                    .OrderBy(ts => ts.StartTime.ToDateTime())
                    .ToList();
            }
            else
            {
                var response = await _doctorService.GetAllDoctorsAsync();
                AvailableDoctors = response.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctor schedule");
            ErrorMessage = "Failed to load schedule.";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Account/Login");
        
            var localDateTime = DateTime.SpecifyKind(AppointmentDateTime, DateTimeKind.Utc);

            Input.PatientId = userId;
            Input.AppointmentDate = Timestamp.FromDateTime(localDateTime.ToUtcTime());

            _logger.LogInformation("Creating appointment for local time: {LocalTime}", localDateTime);
    
            var appointment = await _appointmentService.CreateAppointmentAsync(Input);
            return RedirectToPage("./Details", new { id = appointment.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            ErrorMessage = "Failed to create appointment: " + ex.Message;
            await OnGetAsync(Input.DoctorId);
            return Page();
        }
    }
}