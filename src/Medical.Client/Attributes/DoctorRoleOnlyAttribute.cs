using Microsoft.AspNetCore.Authorization;

namespace Medical.Client.Attributes;

public class DoctorRoleOnlyAttribute : AuthorizeAttribute
{
    public DoctorRoleOnlyAttribute() : base("RequireDoctorRole")
    {
    }
}