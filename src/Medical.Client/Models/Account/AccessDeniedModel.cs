using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Account;

public class AccessDeniedModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}