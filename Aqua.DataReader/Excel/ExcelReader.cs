using MiniExcelLibs;

namespace Aqua.DataReader.Excel;

public static class ExcelReader
{
    public static async Task<List<T>> ReadDataAsync<T>(string filePath) where T : class, new()
    {
        var fullPath = GetFullPath(filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"There is no ExcelFile on: {fullPath}");

        var data = await MiniExcel.QueryAsync<T>(fullPath);
        return data.ToList();
    }

    public static List<T> ReadData<T>(string filePath) where T : class, new()
    {
        var fullPath = GetFullPath(filePath);
        return !File.Exists(fullPath) ? throw new FileNotFoundException($"There is no Excel file at: {fullPath}")
            : MiniExcel.Query<T>(fullPath).ToList();
    }


    public static Task SaveDataAsync<T>(string filePath, IEnumerable<T> data) where T : class, new()
    {
        var fullPath = GetFullPath(filePath);
        EnsureDirectoryExists(fullPath);
        return MiniExcel.SaveAsAsync(fullPath, data, overwriteFile: true);
    }


    public static void SaveData<T>(string filePath, IEnumerable<T> data) where T : class, new()
    {
        var fullPath = GetFullPath(filePath);
        EnsureDirectoryExists(fullPath);
        MiniExcel.SaveAs(fullPath, data, overwriteFile: true);
    }
    
    private static string GetFullPath(string filePath) => Path.Combine(AppContext.BaseDirectory, filePath);
    
    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }
}