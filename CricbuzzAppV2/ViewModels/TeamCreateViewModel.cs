using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class TeamCreateViewModel
{
    [Required]
    public string TeamName { get; set; }

    [Required]
    public string Country { get; set; }

    [Required]
    public string Coach { get; set; }

    public IFormFile? ImageFile { get; set; }

    public string? ImageUrlInput { get; set; }
}
