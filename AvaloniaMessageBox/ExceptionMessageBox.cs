using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

public static class ExceptionMessageBox
{
    public static async Task ShowExceptionDialogAsync(object? parent, Exception ex)
    {
        string exceptionName = ex.GetType().Name;
        string exceptionMessage = ex.Message;

        string? fileName = null;
        int? lineNumber = null;

        try
        {
            StackTrace st = new (ex, true);
            
            StackFrame? firstFrame = st.GetFrames()?.FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.GetFileName()) &&
                f.GetFileLineNumber() > 0);

            if (firstFrame != null)
            {
                fileName = firstFrame.GetFileName();
                lineNumber = firstFrame.GetFileLineNumber();
            }
        }
        catch
        {
            // Ignore reflection or debug info errors
        }

        var cp = CulturePrompt.Current; 

        var sb = new StringBuilder();
        
        sb.AppendLine($"{cp.Exception}: {exceptionName}");
        
        if (!string.IsNullOrWhiteSpace(fileName) && lineNumber.HasValue)
            sb.AppendLine($"{cp.Location}: {System.IO.Path.GetFileName(fileName)}:{lineNumber}");

        sb.AppendLine($"{cp.Message}: {exceptionMessage}");

        await MessageBox.ShowAsync(
            parent,
            cp.AnErrorWasOcurred,
            sb.ToString().Trim(),
            MessageBoxButtons.Ok,
            MessageBoxIcon.Stop
        );
    }
}