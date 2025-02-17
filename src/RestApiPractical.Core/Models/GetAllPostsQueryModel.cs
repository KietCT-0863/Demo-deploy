namespace RestApiPractical.Core.Models;

public class GetAllPostsQueryModel
{
    public string search { get; set; }
    public string created_by { get; set; }
    public string sort_by { get; set; }
    public string sort_order { get; set; }
    public int? page { get; set; }
    public int? page_size { get; set; }
}