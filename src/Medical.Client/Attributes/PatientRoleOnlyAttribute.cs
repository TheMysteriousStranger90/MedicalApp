using Microsoft.AspNetCore.Authorization;

namespace Medical.Client.Attributes;

public class PatientRoleOnlyAttribute : AuthorizeAttribute
{
    public PatientRoleOnlyAttribute() : base("RequirePatientRole")
    {
    }
}