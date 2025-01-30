using System.ComponentModel.DataAnnotations;

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

    [Required]
    public DateTime? DateOfBirth { get; set; }
    
    [Required]
    public string Gender { get; set; }
    
    [Required]
    [Phone]
    public string Phone { get; set; }
    
    [Required]
    public string Address { get; set; }
}