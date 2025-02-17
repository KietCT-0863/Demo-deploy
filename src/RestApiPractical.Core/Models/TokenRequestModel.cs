using System.ComponentModel.DataAnnotations;

namespace RestApiPractical.Core.Models;

public class TokenRequestModel
{
    [Required]
    public string grant_type { get; set; }

    [Required]
    public string username { get; set; }

    [Required]
    public string password { get; set; }
}
