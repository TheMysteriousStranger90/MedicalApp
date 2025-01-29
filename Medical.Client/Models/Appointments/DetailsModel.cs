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
                return NotFound();
            
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


    public async Task<IActionResult> OnPostCompleteAsync(string id)
    {
        try
        {
            var updateRequest = new UpdateAppointmentRequest
            {
                Id = id,
                Status = AppointmentStatus.Completed,
                Notes = Request.Form["Diagnosis"].ToString(),
                Symptoms = Request.Form["Treatment"].ToString()
            };

            await _appointmentService.UpdateAppointmentAsync(updateRequest);

            var createRecordRequest = new CreateMedicalRecordRequest
            {
                PatientId = Appointment.PatientId,
                Diagnosis = Request.Form["Diagnosis"],
                Treatment = Request.Form["Treatment"],
                Prescriptions = Request.Form["Prescriptions"],
                Notes = Request.Form["Notes"]
            };

            await _medicalRecordService.CreateMedicalRecordAsync(createRecordRequest);

            return RedirectToPage("/MedicalRecords/History", new { patientId = Appointment.PatientId });
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to complete appointment";
            return Page();
        }
    }
}