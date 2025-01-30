using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Medical.Client.Validations;

namespace Medical.Client.Models;

public class RegisterInputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    // Doctor fields
    [RequiredIf("IsDoctorEmail", true)]
    public string Specialization { get; set; }
    
    [RequiredIf("IsDoctorEmail", true)]
    public string LicenseNumber { get; set; }
    
    [RequiredIf("IsDoctorEmail", true)]
    public decimal ConsultationFee { get; set; }

    // Patient fields
    [RequiredIf("IsDoctorEmail", false)]
    public DateTime? DateOfBirth { get; set; }
    
    [RequiredIf("IsDoctorEmail", false)]
    public string Gender { get; set; }
    
    [RequiredIf("IsDoctorEmail", false)]
    public string Phone { get; set; }
    
    [RequiredIf("IsDoctorEmail", false)]
    public string Address { get; set; }

    [JsonIgnore]
    public bool IsDoctorEmail => Email?.Contains("@medicalapp", StringComparison.OrdinalIgnoreCase) ?? false;
}