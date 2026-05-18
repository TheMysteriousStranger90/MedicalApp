using Microsoft.AspNetCore.Identity;

namespace Medical.GrpcService.Entities;

public class Role : IdentityRole
{
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}