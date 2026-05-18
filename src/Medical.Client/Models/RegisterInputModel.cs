using System.ComponentModel.DataAnnotations;

namespace Medical.Client.Models;

public class RegisterInputModel
{
    [Required] [EmailAddress] public required string Email { get; set; }

    [Required] public required string FullName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }

    [Required] [Compare("Password")] public required string ConfirmPassword { get; set; }

    [Required] public DateTime? DateOfBirth { get; set; }

    [Required] public required string Gender { get; set; }

    [Required] [Phone] public required string Phone { get; set; }

    [Required] public required string Address { get; set; }
}
