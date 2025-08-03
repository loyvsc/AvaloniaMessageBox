using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

// OpenSource from Castello Branco Tecnologia => Github at 
// https://github.com/CastelloBrancoTecnologia/AvaloniaMessageBox
// MIT License

public static class NativeOsMessageBox
{
    public static async Task<MessageBoxResult> ShowAsync(
        object? parent,
        string caption,
        string text,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (OperatingSystem.IsMacOS())
        {
            return await NativeMessageBoxMacOs.ShowAsync(caption, text, buttons, icon);
        }
        else if (OperatingSystem.IsLinux())
        {
            return NativeMessageBoxLinuxX11.Show(caption, text, buttons, icon);
        }
        else if (OperatingSystem.IsWindows())
        {
            return await NativeMessageBoxWindows.ShowAsync(caption, text, buttons, icon);
        }
        else
        {
            throw new NotSupportedException("Avalonia not available and Native Os not supported." );
        }
    }
    
    public static async Task<MessageBoxResult> ShowExceptionDialogAsync(Exception ex)
    {
        var text = ExceptionHelper.ProvideExceptionDescription(ex);
        var caption = CulturePrompt.Current.AnErrorWasOcurred;
        
        if (OperatingSystem.IsMacOS())
        {
            return await NativeMessageBoxMacOs.ShowAsync(caption, text, MessageBoxButtons.Ok, MessageBoxIcon.Stop);
        }
        else if (OperatingSystem.IsLinux())
        {
            return NativeMessageBoxLinuxX11.Show(caption, text, MessageBoxButtons.Ok, MessageBoxIcon.Stop);
        }
        else if (OperatingSystem.IsWindows())
        {
            return await NativeMessageBoxWindows.ShowAsync(caption, text, MessageBoxButtons.Ok, MessageBoxIcon.Stop);
        }
        else
        {
            throw new NotSupportedException("Avalonia not available and Native Os not supported.");
        }
    }
}