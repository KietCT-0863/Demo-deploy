using System.ComponentModel.DataAnnotations;

namespace RestApiPractical.Core.Models;

public class UpdatePostModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Content { get; set; }
}
