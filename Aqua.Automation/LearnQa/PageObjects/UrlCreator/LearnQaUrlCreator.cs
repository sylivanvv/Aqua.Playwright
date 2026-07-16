using Aqua.AppConfig.Configuration;
using Aqua.Framework.Core;

namespace Aqua.Automation.LearnQa.PageObjects.UrlCreator;

public static class LearnQaUrlCreator
{
    private static IConfig ConfigInstance => AquaServices.Config;
    public static string LearnQaBase => ConfigInstance.Env.LearnQaBaseUrl.TrimEnd('/');
    public static string DragAndDropPage => $"{LearnQaBase}/drag-and-drop/";
    public static string DynamicElementsPage => $"{LearnQaBase}/dynamic-elements/";
    public static string IFramesWindowsPage => $"{LearnQaBase}/iframe-windows/";
    public static string KeyboardMouseEventsPage => $"{LearnQaBase}/keyboard-mouse-events/";
    public static string ShadowRootPage => $"{LearnQaBase}/shadow-dom/";
    public static string FileOperationsPage => $"{LearnQaBase}/file-operations/";
    public static string CapybarasBasePage => $"{LearnQaBase}/self-healing-testing/";
    
    public static class CapybarasApi
    {
        public const string SortedCapybarasEndpoint = "/api/capybaras?page=1&limit=50&mine=true&sort=created_at&order=desc";
    }
}