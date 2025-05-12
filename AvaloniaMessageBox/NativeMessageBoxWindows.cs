using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CastelloBranco.AvaloniaMessageBox;

internal static class NativeMessageBoxWindows
{
    public static Task<MessageBoxResult> ShowAsync(
        string caption,
        string text,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        TaskCompletionSource<MessageBoxResult> tcs = new();

        Task.Run(() =>
        {
            uint type = buttons switch
            {
                MessageBoxButtons.Ok => MB_OK,
                MessageBoxButtons.OkCancel => MB_OKCANCEL,
                MessageBoxButtons.YesNo => MB_YESNO,
                _ => MB_OK
            };

            type |= icon switch
            {
                MessageBoxIcon.Information => MB_ICONINFORMATION,
                MessageBoxIcon.Warning => MB_ICONWARNING,
                MessageBoxIcon.Error => MB_ICONERROR,
                MessageBoxIcon.Question => MB_ICONQUESTION,
                MessageBoxIcon.Stop => MB_ICONERROR,
                MessageBoxIcon.Success => MB_ICONINFORMATION,
                _ => 0
            };

            int result = MessageBox(IntPtr.Zero, text, caption, type);

            MessageBoxResult mappedResult = result switch
            {
                IDOK => MessageBoxResult.Ok,
                IDCANCEL => MessageBoxResult.Cancel,
                IDYES => MessageBoxResult.Yes,
                IDNO => MessageBoxResult.No,
                _ => MessageBoxResult.Ok
            };

            tcs.SetResult(mappedResult);
        });

        return tcs.Task;
    }
    
    // =========================================================================================
    // Win32 PInvoke 

    // Win32 Constants
    private const uint MB_OK = 0x00000000;
    private const uint MB_OKCANCEL = 0x00000001;
    private const uint MB_YESNO = 0x00000004;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint MB_ICONWARNING = 0x00000030;
    private const uint MB_ICONERROR = 0x00000010;
    private const uint MB_ICONQUESTION = 0x00000020;

    private const int IDOK = 1;
    private const int IDCANCEL = 2;
    private const int IDYES = 6;
    private const int IDNO = 7;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}