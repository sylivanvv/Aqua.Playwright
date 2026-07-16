using MiniExcelLibs.Attributes;

namespace Aqua.Automation.LearnQa.Models.Excel;

public record EmployeeModel 
{
    [ExcelColumnName("ID")]
    public int Id { get; set; }

    [ExcelColumnName("Name")]
    public string Name { get; set; }

    [ExcelColumnName("Email")]
    public string Email { get; set; }

    [ExcelColumnName("Score")]
    public decimal Score { get; set; }

    [ExcelColumnName("Date")]
    public string Date { get; set; } 
    public string Department { get; set; }

    [ExcelColumnName("Status")]
    public string Status { get; set; }
};