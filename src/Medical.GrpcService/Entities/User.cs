using Microsoft.AspNetCore.Identity;

namespace Medical.GrpcService.Entities;

public class User : IdentityUser
{
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}