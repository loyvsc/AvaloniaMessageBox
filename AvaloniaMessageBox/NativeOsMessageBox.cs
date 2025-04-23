using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace CastelloBranco.AvaloniaMessageBox;

// OpenSource from Castello Branco Tecnologia => Github at 
// https://github.com/CastelloBrancoTecnologia/AvaloniaMessageBox
// MIT License

public static class NativeOsMessageBox
{
    public static async Task<MessageBoxResult> ShowAsync(
        object? parent,
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (OperatingSystem.IsMacOS())
        {
            return await ShowOnMacOsAsync(title, message, buttons, icon);
        }
        else if (OperatingSystem.IsLinux())
        {
            return await ShowOnLinuxAsync(title, message, buttons, icon);
        }
        else if (OperatingSystem.IsWindows())
        {
            return await ShowOnWindowsAsync(title, message, buttons, icon);
        }
        else
        {
            throw new NotSupportedException("Avalonia not available and Native Os not supported." );
        }
    }
    
    private static Task<MessageBoxResult> ShowOnMacOsAsync(
        string text,
        string caption,
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
                IntPtr alert = CreateNsAlert(caption, text, buttons);

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

    private static Task<MessageBoxResult> ShowOnWindowsAsync(
        string text,
        string caption,
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

    private static Task<MessageBoxResult> ShowOnLinuxAsync(
        string text,
        string caption,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        TaskCompletionSource<MessageBoxResult> tcs = new();

        Task.Run(() =>
        {
            IntPtr display = XOpenDisplay(IntPtr.Zero);

            if (display == IntPtr.Zero)
            {
                tcs.SetException(new InvalidOperationException("Cannot open X display"));
                return;
            }

            IntPtr root = XDefaultRootWindow(display);

            CulturePrompt? cp = CulturePrompt.Current;

            string[] buttonLabels = buttons switch
            {
                MessageBoxButtons.Ok => [cp.Ok],
                MessageBoxButtons.OkCancel => [cp.Ok, cp.Cancela],
                MessageBoxButtons.YesNo => [cp.Sim, cp.Nao],
                _ => [cp.Ok]
            };

            string[] lines = text.Split('\n');
            int lineHeight = 18;
            int lineSpacing = 5;
            int padding = 20;
            int iconSize = 36;
            int iconSpacing = 10;
            int buttonHeight = 30;
            int buttonWidth = 80;
            int buttonSpacing = 10;

            int textWidth = lines.Max(line => line.Length) * 8;
            int contentWidth = icon != MessageBoxIcon.None ? iconSize + iconSpacing + textWidth : textWidth;
            int totalButtonWidth = buttonLabels.Length * buttonWidth + (buttonLabels.Length - 1) * buttonSpacing;
            int width = Math.Max(contentWidth + padding * 2, totalButtonWidth + padding * 2);
            int textHeight = lines.Length * (lineHeight + lineSpacing);
            int contentHeight = Math.Max(textHeight, icon != MessageBoxIcon.None ? iconSize : 0);
            int height = contentHeight + padding * 2 + buttonHeight + padding;

            IntPtr window = XCreateSimpleWindow(display, root, 100, 100, (uint)width, (uint)height, 1, 0, 0xFFFFFF);
            XStoreName(display, window, caption);
            XSelectInput(display, window, ExposureMask | ButtonPressMask);
            XMapRaised(display, window);

            bool quit = false;

            MessageBoxResult result = MessageBoxResult.Ok;

            Dictionary<(int x1, int x2), MessageBoxResult> buttonRegions = new();

            while (!quit)
            {
                XEvent xev;
                XNextEvent(display, out xev);

                switch (xev.type)
                {
                    case 12: // Expose
                        int textStartX = padding;
                        int textStartY = padding + lineHeight;

                        IntPtr gc = XDefaultGC(display, XDefaultScreen(display));
                        IntPtr font = XLoadFont(display, "Symbola");

                        if (font == IntPtr.Zero)
                        {
                            font = XLoadFont(display, "DejaVu Sans" );
                        }
                        
                        if (font != IntPtr.Zero)
                        {
                            XSetFont(display, gc, font);
                        }
                        
                        if (icon != MessageBoxIcon.None)
                        {
                            string iconStr = icon switch
                            {
                                MessageBoxIcon.Information => "ℹ️",
                                MessageBoxIcon.Warning => "⚠️",
                                MessageBoxIcon.Error => "❌",
                                MessageBoxIcon.Question => "❓",
                                MessageBoxIcon.Stop => "🛑",
                                MessageBoxIcon.Success => "✔",
                                _ => ""
                            };

                            int iconY = padding + ((contentHeight - iconSize) / 2) + lineHeight;

                            XDrawString(display, window, gc, padding, iconY, iconStr, iconStr.Length);

                            if (icon == MessageBoxIcon.Stop)
                            {
                                string iconStr2 = "✋";
                                
                                XDrawString(display, window, gc, padding, iconY, iconStr2, iconStr2.Length);
                            }

                            textStartX += iconSize + iconSpacing;
                        }

                        for (int i = 0; i < lines.Length; i++)
                        {
                            XDrawString(display, window, gc, textStartX, padding + (i + 1) * lineHeight, lines[i], lines[i].Length);
                        }

                        int startX = (width - totalButtonWidth) / 2;

                        for (int i = 0; i < buttonLabels.Length; i++)
                        {
                            int bx = startX + i * (buttonWidth + buttonSpacing);
                            int by = height - padding - buttonHeight;
                            string label = "[" + buttonLabels[i] + "]";

                            XDrawString(display, window, gc, bx + 10, by + 20, label, label.Length);

                            MessageBoxResult mapped = MessageBoxResult.Ok;

                            if (buttonLabels[i] == cp.Ok)
                                mapped = MessageBoxResult.Ok;
                            else if (buttonLabels[i] == cp.Cancela)
                                mapped = MessageBoxResult.Cancel;
                            else if (buttonLabels[i] == cp.Sim)
                                mapped = MessageBoxResult.Yes;
                            else if (buttonLabels[i] == cp.Nao)
                                mapped = MessageBoxResult.No;

                            buttonRegions[(bx, bx + buttonWidth)] = mapped;
                        }
                        
                        if (font != IntPtr.Zero)
                        {
                            XUnloadFont(display, font);
                        }

                        break;

                    case 4: // ButtonPress
                        int clickX = xev.xbutton.x;
                        int clickY = xev.xbutton.y;
                        int buttonYStart = height - padding - buttonHeight;
                        if (clickY >= buttonYStart && clickY <= buttonYStart + buttonHeight)
                        {
                            foreach (var region in buttonRegions)
                            {
                                if (clickX >= region.Key.x1 && clickX <= region.Key.x2)
                                {
                                    result = region.Value;
                                    quit = true;
                                    break;
                                }
                            }
                        }

                        break;
                }
            }

            XDestroyWindow(display, window);
            XCloseDisplay(display);

            tcs.SetResult(result);
        });

        return tcs.Task;
    }
    
    // =======================================================================
    // Linux PInvoke

    // X11 Event Structures
    [StructLayout(LayoutKind.Sequential)]
    public struct XEvent
    {
        public int type;
        public XButtonEvent xbutton;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XButtonEvent
    {
        public int type;
        public ulong serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public ulong time;
        public int x, y, x_root, y_root;
        public uint state;
        public uint button;
        public bool same_screen;
    }

    // X11 Native Methods
    
    [DllImport("libX11")]
    public static extern void XUnloadFont(IntPtr display, IntPtr font);

    [DllImport("libX11")]
    public static extern void XSetFont(IntPtr display, IntPtr gc, IntPtr font);

    [DllImport("libX11")]
    public static extern IntPtr XLoadFont(IntPtr display, string fontName);

    [DllImport("libX11")]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11")]
    public static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11")]
    public static extern IntPtr XDefaultRootWindow(IntPtr display);

    [DllImport("libX11")]
    public static extern IntPtr XCreateSimpleWindow(
        IntPtr display, IntPtr parent,
        int x, int y, uint width, uint height,
        uint border_width, ulong border, ulong background);

    [DllImport("libX11")]
    public static extern void XMapRaised(IntPtr display, IntPtr window);

    [DllImport("libX11")]
    public static extern void XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11")]
    public static extern void XStoreName(IntPtr display, IntPtr window, string window_name);

    [DllImport("libX11")]
    public static extern void XSelectInput(IntPtr display, IntPtr window, long event_mask);

    [DllImport("libX11")]
    public static extern void XDrawString(
        IntPtr display, IntPtr window, IntPtr gc,
        int x, int y, string str, int length);

    [DllImport("libX11")]
    public static extern IntPtr XDefaultGC(IntPtr display, int screen_number);

    [DllImport("libX11")]
    public static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11")]
    public static extern void XNextEvent(IntPtr display, out XEvent xevent);

    // X11 Constants
    public const long ExposureMask = 0x00008000;
    public const long ButtonPressMask = 0x00000004;

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
}


