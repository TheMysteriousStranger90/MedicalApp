using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<AppointmentViewModel> Appointments { get; private set; } = new List<AppointmentViewModel>();
    public string? ErrorMessage { get; set; }

    public IndexModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<IndexModel> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                ErrorMessage = "User not authenticated";
                return;
            }

            var request = new AppointmentRequest
            {
                Date = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            // Set filters based on role
            if (userRole == "Patient")
            {
                request.PatientId = userId;
                var appointments = await _appointmentService.GetAppointmentsAsync(request);
                var viewModels = new List<AppointmentViewModel>();
                
                foreach (var appointment in appointments.Where(a => a.PatientId == userId))
                {
                    var doctor = await _doctorService.GetDoctorByIdAsync(appointment.DoctorId);
                    var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
                    
                    viewModels.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate.ToDateTime(),
                        DoctorName = doctor?.FullName ?? "Unknown",
                        PatientName = patient?.FullName ?? "Unknown",
                        Status = appointment.Status
                    });
                }
                
                Appointments = viewModels;
            }
            else if (userRole == "Doctor")
            {
                request.DoctorId = userId;
                var appointments = await _appointmentService.GetAppointmentsAsync(request);
                var viewModels = new List<AppointmentViewModel>();
                
                foreach (var appointment in appointments.Where(a => a.DoctorId == userId))
                {
                    var doctor = await _doctorService.GetDoctorByIdAsync(appointment.DoctorId);
                    var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
                    
                    viewModels.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate.ToDateTime(),
                        DoctorName = doctor?.FullName ?? "Unknown",
                        PatientName = patient?.FullName ?? "Unknown",
                        Status = appointment.Status
                    });
                }
                
                Appointments = viewModels;
            }
            else if (User.IsInRole("Admin"))
            {
                var appointments = await _appointmentService.GetAppointmentsAsync(request);
                var viewModels = new List<AppointmentViewModel>();
                
                foreach (var appointment in appointments)
                {
                    var doctor = await _doctorService.GetDoctorByIdAsync(appointment.DoctorId);
                    var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
                    
                    viewModels.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate.ToDateTime(),
                        DoctorName = doctor?.FullName ?? "Unknown",
                        PatientName = patient?.FullName ?? "Unknown",
                        Status = appointment.Status
                    });
                }
                
                Appointments = viewModels;
            }
            else
            {
                ErrorMessage = "Unauthorized access";
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for user {UserId}", 
                User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            ErrorMessage = "Failed to load appointments.";
        }
    }
}