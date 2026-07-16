namespace Aqua.Framework.Utils;

public static class PathProvider
{
    private static readonly string RootArtifactsDir = Path.Combine(AppContext.BaseDirectory, "TestResults");
    
    public static string ScreenshotsPath => field ??= GetOrCreateFolder("Screenshots");
    
    public static string LogsPath => field ??= GetOrCreateFolder("Logs");
    
    public static string DownloadsPath => field ??= GetOrCreateFolder("Downloads");
    
    public static string ArtifactsPath => field ??= GetOrCreateFolder("Artifacts");
    
    private static string GetOrCreateFolder(string folderName)
    {
        var path = Path.Combine(RootArtifactsDir, folderName);
        Directory.CreateDirectory(path); 
        return path + Path.DirectorySeparatorChar;
    }
}