using System.Diagnostics;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

internal static class ExceptionHelper
{
    public static string ProvideExceptionDescription(Exception ex)
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

        return sb.ToString().Trim();
    }
}