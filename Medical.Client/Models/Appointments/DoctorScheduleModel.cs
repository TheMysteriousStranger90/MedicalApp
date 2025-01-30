using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class DoctorScheduleModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly ILogger<DoctorScheduleModel> _logger;

    public IEnumerable<AppointmentViewModel> Appointments { get; private set; } = new List<AppointmentViewModel>();
    [BindProperty(SupportsGet = true)] public DateTime? StartDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? EndDate { get; set; }
    public string? ErrorMessage { get; set; }

    public DoctorScheduleModel(
        IAppointmentService appointmentService,
        IPatientService patientService,
        ILogger<DoctorScheduleModel> logger)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var doctorId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
            {
                ErrorMessage = "Doctor ID not found";
                return;
            }

            StartDate ??= DateTime.Today;
            EndDate ??= DateTime.Today.AddDays(7);

            var request = new AppointmentRequest
            {
                DoctorId = doctorId,
                Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(StartDate.Value.ToUniversalTime())
            };

            var appointments = await _appointmentService.GetAppointmentsAsync(request);
            var viewModels = new List<AppointmentViewModel>();

            foreach (var appointment in appointments)
            {
                var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
                viewModels.Add(new AppointmentViewModel
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate.ToDateTime().ToLocalTime(),
                    PatientName = patient?.FullName ?? "Unknown",
                    Status = appointment.Status
                });
            }

            Appointments = viewModels
                .Where(a => a.AppointmentDate.Date >= StartDate.Value.Date && 
                           a.AppointmentDate.Date <= EndDate.Value.Date)
                .OrderBy(a => a.AppointmentDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctor schedule");
            ErrorMessage = "Failed to load schedule.";
        }
    }
}