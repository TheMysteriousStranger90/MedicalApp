using System.Security.Claims;
using Medical.Client.Attributes;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Patients;

[DoctorRoleOnly]
public class MyPatientsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly ILogger<MyPatientsModel> _logger;

    public IEnumerable<PatientViewModel> UpcomingPatients { get; private set; } = new List<PatientViewModel>();
    public IEnumerable<PatientViewModel> PastPatients { get; private set; } = new List<PatientViewModel>();
    public string? ErrorMessage { get; set; }

    public MyPatientsModel(
        IAppointmentService appointmentService,
        IPatientService patientService,
        ILogger<MyPatientsModel> logger)
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

            var request = new AppointmentRequest { DoctorId = doctorId };
            var appointments = await _appointmentService.GetAppointmentsAsync(request);

            var upcomingPatientIds = appointments
                .Where(a => a.AppointmentDate.ToDateTime() > DateTime.Now && 
                           a.Status == AppointmentStatus.Scheduled)
                .Select(a => a.PatientId)
                .Distinct();

            var pastPatientIds = appointments
                .Where(a => a.AppointmentDate.ToDateTime() <= DateTime.Now || 
                           a.Status != AppointmentStatus.Scheduled)
                .Select(a => a.PatientId)
                .Distinct();

            var upcomingPatients = new List<PatientViewModel>();
            var pastPatients = new List<PatientViewModel>();

            foreach (var patientId in upcomingPatientIds)
            {
                var patient = await _patientService.GetPatientByIdAsync(patientId);
                var nextAppointment = appointments
                    .Where(a => a.PatientId == patientId && 
                           a.AppointmentDate.ToDateTime() > DateTime.Now)
                    .OrderBy(a => a.AppointmentDate)
                    .FirstOrDefault();

                upcomingPatients.Add(new PatientViewModel
                {
                    Id = patient.Id,
                    FullName = patient.FullName,
                    Phone = patient.Phone,
                    DateOfBirth = patient.DateOfBirth,
                    NextAppointment = nextAppointment?.AppointmentDate.ToDateTime()
                });
            }

            foreach (var patientId in pastPatientIds)
            {
                var patient = await _patientService.GetPatientByIdAsync(patientId);
                var lastAppointment = appointments
                    .Where(a => a.PatientId == patientId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefault();

                pastPatients.Add(new PatientViewModel
                {
                    Id = patient.Id,
                    FullName = patient.FullName,
                    Phone = patient.Phone,
                    DateOfBirth = patient.DateOfBirth,
                    LastAppointment = lastAppointment?.AppointmentDate.ToDateTime()
                });
            }

            UpcomingPatients = upcomingPatients;
            PastPatients = pastPatients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients");
            ErrorMessage = "Failed to load patients. Please try again later.";
        }
    }
}