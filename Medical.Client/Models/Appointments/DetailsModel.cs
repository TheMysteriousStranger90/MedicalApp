using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Appointments;

public class DetailsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    [BindProperty]
    public CompleteAppointmentModel CompleteModel { get; set; }
    public AppointmentModel? Appointment { get; private set; }
    public string? DoctorName { get; private set; }
    public string? PatientName { get; private set; }
    public string? ErrorMessage { get; set; }

    public DetailsModel(
        IAppointmentService appointmentService,
        IMedicalRecordService medicalRecordService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<DetailsModel> logger)
    {
        _appointmentService = appointmentService;
        _medicalRecordService = medicalRecordService;
        _doctorService = doctorService;
        _patientService = patientService;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (Appointment == null)
            {
                ErrorMessage = "Appointment not found.";
                return Page();
            }

            if (!User.IsInRole("Admin") &&
                Appointment.DoctorId != userId &&
                Appointment.PatientId != userId)
            {
                return Forbid();
            }

            var doctor = await _doctorService.GetDoctorByIdAsync(Appointment.DoctorId);
            var patient = await _patientService.GetPatientByIdAsync(Appointment.PatientId);

            DoctorName = doctor?.FullName;
            PatientName = patient?.FullName;

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load appointment details.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCompleteAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill in all required fields";
                return Page();
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(CompleteModel.AppointmentId);
            if (appointment == null)
            {
                ErrorMessage = "Appointment not found";
                return Page();
            }
            
            var updateRequest = new UpdateAppointmentRequest
            {
                Id = CompleteModel.AppointmentId,
                Status = AppointmentStatus.Completed,
                Notes = CompleteModel.Diagnosis,
                Symptoms = CompleteModel.Treatment,
                IsPaid = appointment.IsPaid,
                Fee = appointment.Fee
            };

            await _appointmentService.UpdateAppointmentAsync(updateRequest);

            var createRecordRequest = new CreateMedicalRecordRequest
            {
                PatientId = appointment.PatientId,
                Diagnosis = CompleteModel.Diagnosis,
                Treatment = CompleteModel.Treatment,
                Prescriptions = CompleteModel.Prescriptions ?? string.Empty,
                Notes = CompleteModel.Notes ?? string.Empty
            };

            await _medicalRecordService.CreateMedicalRecordAsync(createRecordRequest);

            return RedirectToPage("/MedicalRecords/History", new { patientId = appointment.PatientId });
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to complete appointment";
            return Page();
        }
    }
}