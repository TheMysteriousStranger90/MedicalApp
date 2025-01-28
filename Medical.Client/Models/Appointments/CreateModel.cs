using System.Security.Claims;
using Grpc.Core;
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

    [BindProperty] public CreateAppointmentRequest Input { get; set; } = new();
    public List<DoctorModel> AvailableDoctors { get; set; } = new();
    public DoctorModel? SelectedDoctor { get; set; }
    public string? ErrorMessage { get; set; }

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
            }
            else
            {
                var response = await _doctorService.GetAllDoctorsAsync();
                AvailableDoctors = response.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctors");
            ErrorMessage = "Failed to load available doctors.";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            Input.PatientId = userId;

            if (SelectedDoctor != null)
            {
                Input.Fee = SelectedDoctor.ConsultationFee;
            }

            var appointment = await _appointmentService.CreateAppointmentAsync(Input);
            return RedirectToPage("./Details", new { id = appointment.Id });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            _logger.LogWarning("Authentication failed during appointment creation");
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            ErrorMessage = "Failed to create appointment. Please try again.";
            return Page();
        }
    }
}