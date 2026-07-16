namespace Aqua.Automation.LearnQa.Models.Capybara;

public class CapybaraResponse
{
    public List<CapybaraModel> Capybaras { get; set; } = [];
    public Pagination Pagination { get; set; } = new();
}

public class Pagination
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}