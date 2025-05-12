
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CastelloBranco.AvaloniaMessageBox;

internal static class NativeMessageBoxMacOs
{
    public static Task<MessageBoxResult> ShowAsync( string caption,
                                                     string text,
                                                     MessageBoxButtons buttons = MessageBoxButtons.Ok,
                                                     MessageBoxIcon icon = MessageBoxIcon.None)
    {
        TaskCompletionSource<MessageBoxResult> tcs = new();

        _ = Task.Run(() =>
        {
            MessageBoxResult result = MessageBoxResult.None;

            try
            {
                // Use AppKit to display a modal dialog
                IntPtr alert = CreateNsAlert(caption, text, buttons, icon);

                IntPtr runModalSel = sel_registerName("runModal");

                int response = (int)objc_msgSend(alert, runModalSel);

                result = buttons switch
                {
                    MessageBoxButtons.Ok =>
                        response switch
                        {
                            1000 => MessageBoxResult.Ok, // NSAlertFirstButtonReturn
                            _ => MessageBoxResult.Ok
                        },

                    MessageBoxButtons.OkCancel =>
                        response switch
                        {
                            1000 => MessageBoxResult.Ok, // NSAlertFirstButtonReturn
                            1001 => MessageBoxResult.Cancel, // NSAlertSecondButtonReturn
                            _ => MessageBoxResult.Ok
                        },

                    MessageBoxButtons.YesNo =>
                        response switch
                        {
                            1000 => MessageBoxResult.Yes, // NSAlertThirdButtonReturn
                            1001 => MessageBoxResult.No,
                            _ => MessageBoxResult.Ok
                        },

                    _ => MessageBoxResult.Ok
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show message box on macOS: {ex.Message}");

                throw new InvalidOperationException("Failed to show message box on macOS.", ex);
            }

            tcs.SetResult(result);
        });

        return tcs.Task;
    }
    
    // =========================================================================================
    // MacOs Utility functions
    private static IntPtr NsStringFromString(string str)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr stringWithUtf8Sel = sel_registerName("stringWithUTF8String:");
        IntPtr utf8Str = Marshal.StringToHGlobalAuto(str);

        IntPtr nsString = objc_msgSend(nsStringClass, stringWithUtf8Sel, utf8Str);

        Marshal.FreeHGlobal(utf8Str);

        return nsString;
    }

    private static IntPtr CreateNsAlert(string title, string message, MessageBoxButtons buttons,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        // Get the NSAlert class and selectors
        IntPtr nsAlertClass = objc_getClass("NSAlert");
        if (nsAlertClass == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to get NSAlert class.");
        }

        IntPtr allocSel = sel_registerName("alloc");
        IntPtr initSel = sel_registerName("init");
        IntPtr setMessageTextSel = sel_registerName("setMessageText:");
        IntPtr setInformativeTextSel = sel_registerName("setInformativeText:");
        IntPtr setIconSel = sel_registerName("setIcon:");
        IntPtr addButtonWithTitleSel = sel_registerName("addButtonWithTitle:");

        // Ativar o app antes de exibir o alerta
        IntPtr nsAppClass = objc_getClass("NSApplication");
        IntPtr sharedAppSel = sel_registerName("sharedApplication");
        IntPtr activateIgnoringOtherAppsSel = sel_registerName("activateIgnoringOtherApps:");
        IntPtr sharedApp = objc_msgSend(nsAppClass, sharedAppSel);
        objc_msgSend(sharedApp, activateIgnoringOtherAppsSel, 1);

        // Create an instance of NSAlert
        IntPtr alert = objc_msgSend(objc_msgSend(nsAlertClass, allocSel), initSel);

        // Set the message text
        IntPtr nsTitle = NsStringFromString(title);
        objc_msgSend(alert, setMessageTextSel, nsTitle);

        // Set the informative text
        IntPtr nsMessage = NsStringFromString(message);
        objc_msgSend(alert, setInformativeTextSel, nsMessage);

        // 🌟 Novo bloco: adicionar ícone com base no tipo
        if (icon != MessageBoxIcon.None)
        {
            string? nsImageName = icon switch
            {
                MessageBoxIcon.Information => "NSInfo",
                MessageBoxIcon.Warning => "NSCaution",
                MessageBoxIcon.Error => "NSCritical",
                MessageBoxIcon.Question => "NSHelp", // ou "NSInfo" se preferir
                MessageBoxIcon.Stop => "NSStopProgressTemplate",
                MessageBoxIcon.Success => "NSMenuOnStateTemplate", // não existe nativo real, este é apenas sugestão
                _ => null
            };

            if (!string.IsNullOrEmpty(nsImageName))
            {
                IntPtr nsImageClass = objc_getClass("NSImage");
                IntPtr imageNamedSel = sel_registerName("imageNamed:");
                IntPtr nsImageNameStr = NsStringFromString(nsImageName);
                IntPtr nsImage = objc_msgSend(nsImageClass, imageNamedSel, nsImageNameStr);

                if (nsImage != IntPtr.Zero)
                    objc_msgSend(alert, setIconSel, nsImage);
            }
        }

        CulturePrompt? cp = CulturePrompt.Current; 

        // Add buttons based on the MessageBoxButtons
        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                objc_msgSend(alert, addButtonWithTitleSel, NsStringFromString(cp.Ok));
                break;

            case MessageBoxButtons.OkCancel:
                objc_msgSend(alert, addButtonWithTitleSel, NsStringFromString(cp.Ok));
                objc_msgSend(alert, addButtonWithTitleSel, NsStringFromString(cp.Cancela));
                break;

            case MessageBoxButtons.YesNo:
                objc_msgSend(alert, addButtonWithTitleSel, NsStringFromString(cp.Sim));
                objc_msgSend(alert, addButtonWithTitleSel, NsStringFromString(cp.Nao));
                break;

            default:
                throw new NotSupportedException($"MessageBoxButtons {buttons} is not supported.");
        }

        return alert;
    }
        
    // =======================================================================
    // Mac OS PInvoke

    [DllImport("libobjc.dylib")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("libobjc.dylib")]
    private static extern IntPtr sel_registerName(string selectorName);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);
}