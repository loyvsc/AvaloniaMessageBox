using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

public static class ExceptionMessageBox
{
    public static async Task ShowExceptionDialogAsync(object? parent, Exception ex)
    {
        var text = ExceptionHelper.ProvideExceptionDescription(ex);
        var caption = CulturePrompt.Current.AnErrorWasOcurred;

        await MessageBox.ShowAsync(
            parent,
            caption,
            text,
            MessageBoxButtons.Ok,
            MessageBoxIcon.Stop
        );
    }
}