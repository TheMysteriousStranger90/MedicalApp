using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<AppointmentModel> Appointments { get; private set; } = new List<AppointmentModel>();
    public string? ErrorMessage { get; set; }

    public IndexModel(IAppointmentService appointmentService, ILogger<IndexModel> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var request = new AppointmentRequest();
            Appointments = await _appointmentService.GetAppointmentsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            ErrorMessage = "Failed to load appointments.";
        }
    }
}