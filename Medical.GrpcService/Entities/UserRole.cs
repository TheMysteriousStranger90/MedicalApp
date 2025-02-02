﻿using Microsoft.AspNetCore.Identity;

namespace Medical.GrpcService.Entities;

public class UserRole : IdentityUserRole<string>
{
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}