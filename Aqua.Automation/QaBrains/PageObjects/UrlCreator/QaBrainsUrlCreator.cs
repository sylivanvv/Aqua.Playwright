namespace Aqua.Automation.QaBrains.PageObjects.UrlCreator;

using AppConfig.Configuration;
using Framework.Core;


public static class QaBrainsUrlCreator
{
    private static IConfig ConfigInstance => AquaServices.Config;
    public static string QaBrainsBase => ConfigInstance.Env.QaBrainsBaseUrl.TrimEnd('/');
}