using System.ComponentModel.DataAnnotations;

namespace RestApiPractical.Core.Models;

public class UpdateCommentModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Content { get; set; }
}
