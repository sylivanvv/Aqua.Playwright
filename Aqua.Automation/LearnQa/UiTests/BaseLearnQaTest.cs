namespace Aqua.Automation.LearnQa.UiTests;

public abstract class BaseLearnQaTest : BasePlaywrightTest
{
    [SetUp]
    public virtual async Task InitAsync()
    {
        var currentTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        await Page.AddInitScriptAsync($$"""
                                                window.localStorage.setItem(
                                                    'cookie-consent', 
                                                    '{"essential": true,  "timestamp": "{{currentTimestamp}}"}'
                                                );
                                        """);
    }
}