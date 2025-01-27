using System.ComponentModel.DataAnnotations;

namespace Medical.Client.Models;

public class RegisterInputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }
}