using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class DoctorScheduleModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<DoctorScheduleModel> _logger;

    public IEnumerable<AppointmentModel> Appointments { get; private set; } = new List<AppointmentModel>();
    [BindProperty(SupportsGet = true)] public DateTime? StartDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? EndDate { get; set; }
    public string? ErrorMessage { get; set; }

    public DoctorScheduleModel(IAppointmentService appointmentService, ILogger<DoctorScheduleModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var doctorId = User.Claims.FirstOrDefault(c => c.Type == "DoctorId")?.Value;
            if (string.IsNullOrEmpty(doctorId))
                return;

            var request = new AppointmentRequest
            {
                DoctorId = doctorId,
                Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(StartDate?.ToUniversalTime() ??
                                                                             DateTime.UtcNow)
            };

            Appointments = await _appointmentService.GetAppointmentsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctor schedule");
            ErrorMessage = "Failed to load schedule.";
        }
    }
}