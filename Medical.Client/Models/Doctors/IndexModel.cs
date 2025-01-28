using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical.Client.Models.Doctors;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<IndexModel> _logger;
    public IEnumerable<DoctorModel> Doctors { get; private set; } = new List<DoctorModel>();
    public string? ErrorMessage { get; set; }
    [BindProperty(SupportsGet = true)] public string? Specialization { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; }
    [BindProperty(SupportsGet = true)] public string SortOrder { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalPages { get; private set; }

    public IndexModel(IDoctorService doctorService, ILogger<IndexModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }
    
    private string GetLastName(string fullName)
    {
        var parts = fullName.Split(' ');
        return parts.Length > 1 ? parts[^1] : fullName;
    }
    
    private int GetExperienceYears(string experience)
    {
        try
        {
            var years = experience.ToLower()
                .Replace("years", "")
                .Replace("year", "")
                .Trim();
            
            return int.TryParse(years, out int result) ? result : 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task OnGetAsync()
    {
        try
        {
            var doctors = await _doctorService.GetDoctorsBySpecializationAsync(Specialization ?? string.Empty);

            if (!string.IsNullOrEmpty(Specialization))
            {
                doctors = doctors
                    .Where(d => d.Specialization.Contains(Specialization, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            switch (SortBy?.ToLower())
            {
                case "name":
                    doctors = SortOrder == "desc"
                        ? doctors.OrderByDescending(d => GetLastName(d.FullName))
                            .ThenByDescending(d => d.FullName)
                            .ToList()
                        : doctors.OrderBy(d => GetLastName(d.FullName))
                            .ThenBy(d => d.FullName)
                            .ToList();
                    break;
                case "experience":
                    doctors = SortOrder == "desc"
                        ? doctors.OrderByDescending(d => GetExperienceYears(d.Experience))
                            .ThenByDescending(d => d.FullName)
                            .ToList()
                        : doctors.OrderBy(d => GetExperienceYears(d.Experience))
                            .ThenBy(d => d.FullName)
                            .ToList();
                    break;
                case "fee":
                    doctors = SortOrder == "desc"
                        ? doctors.OrderByDescending(d => d.ConsultationFee).ToList()
                        : doctors.OrderBy(d => d.ConsultationFee).ToList();
                    break;
                default:
                    doctors = doctors.OrderBy(d => d.FullName).ToList();
                    break;
            }

            TotalPages = (int)Math.Ceiling(doctors.Count() / (double)PageSize);
            Doctors = doctors.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors");
            ErrorMessage = "Failed to load doctors. Please try again later.";
        }
    }
}