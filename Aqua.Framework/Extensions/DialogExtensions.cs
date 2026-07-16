using Microsoft.Playwright;

namespace Aqua.Framework.Extensions;

public static class DialogExtensions
{
    public static async Task<string?> RunAndHandleDialogAsync(
        this IPage page, 
        Func<Task> triggerAction, 
        bool accept = true, 
        string? promptText = null)
    {
        string? dialogMessage = null;

        page.Dialog += Handler;
    
        try
        {
            await triggerAction();
        }
        finally
        {
            page.Dialog -= Handler;
        }

        return dialogMessage;

        async void Handler(object? sender, IDialog dialog)
        {
            try 
            {
                dialogMessage = dialog.Message;
            
                if (accept) 
                    await dialog.AcceptAsync(promptText);
                else 
                    await dialog.DismissAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}