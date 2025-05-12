using System.Runtime.InteropServices;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

internal class NativeMessageBoxLinuxX11
{
     private const int LineHeight = 20;
    private const int Padding = 20;
    private const int IconSize = 36;
    private const int IconSpacing = 10;
    private const int ButtonHeight = 30;
    private const int ButtonWidth = 100;
    private const int ButtonSpacing = 10;

    private readonly CulturePrompt _cp = CulturePrompt.Current;
    private readonly string _caption;
    private readonly MessageBoxIcon _icon;

    private IntPtr _display = IntPtr.Zero;
    private int _screen;
    private IntPtr _root = IntPtr.Zero;
    private IntPtr _visual = IntPtr.Zero;
    private IntPtr _colormap = IntPtr.Zero;

    private readonly string[] _buttonLabels;
    private readonly string[] _lines;

    private readonly int _totalButtonWidth;
    private readonly int _width;
    private readonly int _contentHeight;
    private readonly int _height;
    private IntPtr _window;
    private int _focusedButton;
    private readonly Dictionary<int, (int x1, int x2, int y1, int y2)> _buttonBounds;
    private readonly Dictionary<int, MessageBoxResult> _buttonResults;
    private MessageBoxResult? _cancelResult;
    private MessageBoxResult _result;

    public static MessageBoxResult Show(string caption,
        string text,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
            NativeMessageBoxLinuxX11 mb = new(caption, text, buttons, icon);
            if (!mb.Show())
                throw new InvalidOperationException("Cannot open X display");

            mb.Run();
            mb.Close();
            return mb._result;
    }

    private NativeMessageBoxLinuxX11(string caption, string text, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        _caption = caption;
        _icon = icon;

        _lines = text.Split('\n');
        _buttonLabels = buttons switch
        {
            MessageBoxButtons.Ok => [_cp.Ok],
            MessageBoxButtons.OkCancel => [_cp.Ok, _cp.Cancela],
            MessageBoxButtons.YesNo => [_cp.Sim, _cp.Nao],
            _ => [_cp.Ok]
        };

        var textWidth = _lines.Max(l => l.Length) * 10;
        var contentWidth = icon != MessageBoxIcon.None ? IconSize + IconSpacing + textWidth : textWidth;
        _totalButtonWidth = _buttonLabels.Length * ButtonWidth + (_buttonLabels.Length - 1) * ButtonSpacing;
        _width = Math.Max(contentWidth + Padding * 2, _totalButtonWidth + Padding * 2);
        var textHeight = _lines.Length * LineHeight;
        _contentHeight = Math.Max(textHeight, icon != MessageBoxIcon.None ? IconSize : 0);
        _height = _contentHeight + Padding * 2 + ButtonHeight + Padding;

        _focusedButton = 0;
        _result = MessageBoxResult.Ok;

        _buttonBounds = new();
        _buttonResults = new();
    }

    private bool Show()
    {
        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero) return false;

        _screen = XDefaultScreen(_display);
        
        _root = XDefaultRootWindow(_display);
        _visual = XDefaultVisual(_display, _screen);
        _colormap = XDefaultColormap(_display, _screen);
        
        XSetWindowAttributes attr = new()
        {
            override_redirect = true
        };
        
        ulong mask = (1 << 1); // CWOverrideRedirect 

        _window = XCreateWindow(_display, _root, 
                                100, 100, (uint)_width, (uint)_height, 
                                0, 0, 1, _visual,
                                 mask, ref attr);
        
        // Try to remove decorations using _MOTIF_WM_HINTS (may work under X11, usually ignored under XWayland)

        MotifWmHints hints = new()
        {
            flags = MWM_HINTS_DECORATIONS,
            decorations = 0 // 0 = no decorations
        };

        IntPtr atomHints = XInternAtom(_display, "_MOTIF_WM_HINTS", false);

        XChangeProperty(_display, _window, atomHints, atomHints, 32, 0, // PropModeReplace
                        ref hints, PROP_MOTIF_WM_HINTS_ELEMENTS);
        
        XStoreName(_display, _window, _caption);
        XSelectInput(_display, _window, ExposureMask | ButtonPressMask | KeyPressMask | MotionNotifyMask);
        XMapRaised(_display, _window);
        XFlush(_display);

        XEvent ev;
        do XNextEvent(_display, out ev); while (ev.type != 12);

        DrawMessageBox();
        
        return true;
    }

    private void Close()
    {
        if (_display != IntPtr.Zero && _window != IntPtr.Zero)
        {
            XClearWindow(_display, _window);
            XDestroyWindow(_display, _window);
            XFlush(_display);
            XSync(_display, false);
            XCloseDisplay(_display);
        }
    }

    private const ulong MWM_HINTS_DECORATIONS = 1 << 1;
    private const ulong PROP_MOTIF_WM_HINTS_ELEMENTS = 5;
    
    [DllImport("libX11.so.6")]
    private static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(
        IntPtr display,
        IntPtr w,
        IntPtr property,
        IntPtr type,
        int format,
        int mode,
        ref MotifWmHints data,
        ulong nelements);

    [StructLayout(LayoutKind.Sequential)]
    private struct MotifWmHints
    {
        public ulong flags;
        public ulong functions;
        public ulong decorations;
        public long input_mode;
        public ulong status;
    }
    
    private void Run()
    {
        bool quit = false;
        while (!quit)
        {
            XNextEvent(_display, out XEvent xev);

            switch (xev.type)
            {
                case 12:
                    DrawMessageBox();
                    break;

                case 2: // KeyPress
                    uint keycode = xev.xkey.keycode;
                    bool shift = (xev.xkey.state & 1) != 0;

                    if (keycode == 36 || keycode == 65) // Enter or Space
                    {
                        _result = _buttonResults[_focusedButton];
                        quit = true;
                    }
                    else if (keycode == 9) // Escape
                    {
                        if (_cancelResult.HasValue)
                        {
                            _result = _cancelResult.Value;
                            quit = true;
                        }
                        else if (_buttonLabels.Length == 1 && _buttonResults.ContainsValue(MessageBoxResult.Ok))
                        {
                            _result = MessageBoxResult.Ok;
                            quit = true;
                        }
                    }
                    else if (keycode == 23 || keycode == 62) // Tab or Shift+Tab
                    {
                        _focusedButton = shift
                            ? (_focusedButton - 1 + _buttonLabels.Length) % _buttonLabels.Length
                            : (_focusedButton + 1) % _buttonLabels.Length;
                        DrawMessageBox();
                    }
                    break;
                case 4: // ButtonPress
                    int cx = xev.xbutton.x, cy = xev.xbutton.y;
                    foreach (var (idx, bounds) in _buttonBounds)
                    {
                        var (x1, x2, y1, y2) = bounds;
                        if (cx >= x1 && cx <= x2 && cy >= y1 && cy <= y2)
                        {
                            _result = _buttonResults[idx];
                            quit = true;
                            break;
                        }
                    }
                    break;

                case 6: // MotionNotify
                    int mx = xev.xmotion.x, my = xev.xmotion.y;
                    for (int i = 0; i < _buttonLabels.Length; i++)
                    {
                        var (x1, x2, y1, y2) = _buttonBounds[i];
                        if (mx >= x1 && mx <= x2 && my >= y1 && my <= y2)
                        {
                            if (_focusedButton != i)
                            {
                                _focusedButton = i;
                                DrawMessageBox();
                            }
                            break;
                        }
                    }
                    break;
            }
        }
    }
    
    // private void DrawMessageBox()
    // {
    //     IntPtr draw = XftDrawCreate(_display, _window, _visual, _colormap);
    //     IntPtr font = XftFontOpenName(_display, _screen, "DejaVu Sans-12");
    //
    //     var fg = new XRenderColor { red = 0, green = 0, blue = 0, alpha = 65535 };
    //     var bg = new XRenderColor { red = 65535, green = 65535, blue = 65535, alpha = 65535 };
    //     var invert = new XRenderColor { red = 65535, green = 65535, blue = 65535, alpha = 65535 };
    //     XftColorAllocValue(_display, _visual, _colormap, ref fg, out var textColor);
    //     XftColorAllocValue(_display, _visual, _colormap, ref bg, out var bgColor);
    //     XftColorAllocValue(_display, _visual, _colormap, ref invert, out var invertColor);
    //
    //     int textStartX = Padding;
    //     
    //     if (_icon != MessageBoxIcon.None)
    //     {
    //         string iconStr = _icon switch
    //         {
    //             MessageBoxIcon.Information => "\u2139", // ℹ️
    //             MessageBoxIcon.Warning => "\u26A0", // ⚠️
    //             MessageBoxIcon.Error => "\u274C", // ❌
    //             MessageBoxIcon.Question => "\u2753", // ❓
    //             MessageBoxIcon.Stop => "\u1F6D1", // 🛑
    //             MessageBoxIcon.Success => "\u2714", // ✔
    //             _ => ""
    //         };
    //         byte[] utf8Icon = Encoding.UTF8.GetBytes(iconStr);
    //         int iconY = Padding + ((_contentHeight - IconSize) / 2) + LineHeight;
    //         XftDrawStringUtf8(draw, ref textColor, font, Padding, iconY, utf8Icon, utf8Icon.Length);
    //         textStartX += IconSize + IconSpacing;
    //     }
    //
    //     for (int i = 0; i < _lines.Length; i++)
    //     {
    //         byte[] utf8Line = Encoding.UTF8.GetBytes(_lines[i]);
    //         int lineY = Padding + (i + 1) * LineHeight;
    //         XftDrawStringUtf8(draw, ref textColor, font, textStartX, lineY, utf8Line, utf8Line.Length);
    //     }
    //
    //     int startX = (_width - _totalButtonWidth) / 2;
    //     int by = _height - Padding - ButtonHeight;
    //
    //     _buttonBounds.Clear();
    //     _buttonResults.Clear();
    //
    //     for (int i = 0; i < _buttonLabels.Length; i++)
    //     {
    //         int bx = startX + i * (ButtonWidth + ButtonSpacing);
    //         string label = "[" + _buttonLabels[i] + "]";
    //         byte[] utf8Label = Encoding.UTF8.GetBytes(label);
    //
    //         var isFocused = (i == _focusedButton);
    //         var fgColor = isFocused ? invertColor : textColor;
    //
    //         XftDrawRect(draw, isFocused ? textColor : bgColor, bx, by, ButtonWidth, ButtonHeight);
    //         XftDrawStringUtf8(draw, ref fgColor, font, bx + 10, by + 20, utf8Label, utf8Label.Length);
    //
    //         MessageBoxResult mapped = _buttonLabels[i] switch
    //         {
    //             var b when b == _cp.Ok => MessageBoxResult.Ok,
    //             var b when b == _cp.Cancela => MessageBoxResult.Cancel,
    //             var b when b == _cp.Sim => MessageBoxResult.Yes,
    //             var b when b == _cp.Nao => MessageBoxResult.No,
    //             _ => MessageBoxResult.Ok
    //         };
    //
    //         if (mapped == MessageBoxResult.Cancel)
    //             _cancelResult = mapped;
    //
    //         _buttonBounds[i] = (bx, bx + ButtonWidth, by, by + ButtonHeight);
    //         _buttonResults[i] = mapped;
    //     }
    //
    //     XftFontClose(_display, font);
    //     XftDrawDestroy(draw);
    //     XFlush(_display);
    // }
    
    private void DrawMessageBox()
    {
        IntPtr draw = XftDrawCreate(_display, _window, _visual, _colormap);
        IntPtr font = XftFontOpenName(_display, _screen, "DejaVu Sans-12");

        if (font == IntPtr.Zero) throw new InvalidOperationException("Failed to load font.");
        
        var fg = new XRenderColor { red = 0, green = 0, blue = 0, alpha = 65535 };
        var bg = new XRenderColor { red = 65535, green = 65535, blue = 65535, alpha = 65535 };
        var invert = new XRenderColor { red = 65535, green = 65535, blue = 65535, alpha = 65535 };
        
        XftColorAllocValue(_display, _visual, _colormap, ref fg, out var textColor);
        XftColorAllocValue(_display, _visual, _colormap, ref bg, out var bgColor);
        XftColorAllocValue(_display, _visual, _colormap, ref invert, out var invertColor);

        // Set window background to white (fill entire area):
        XftDrawRect(draw, bgColor, 0, 0, _width, _height);
        
        int textStartX = Padding;

        if (_icon != MessageBoxIcon.None)
        {
            string iconStr = _icon switch
            {
                MessageBoxIcon.Information => "\u2139",
                MessageBoxIcon.Warning => "\u26A0",
                MessageBoxIcon.Error => "\u274C",
                MessageBoxIcon.Question => "\u2753",
                MessageBoxIcon.Stop => "\U0001F6D1",
                MessageBoxIcon.Success => "\u2714",
                _ => ""
            };
            byte[] utf8Icon = Encoding.UTF8.GetBytes(iconStr);
            int iconY = Padding + ((_contentHeight - IconSize) / 2) + IconSize / 2;
            XftDrawStringUtf8(draw, ref textColor, font, Padding, iconY, utf8Icon, utf8Icon.Length);
            textStartX += IconSize + IconSpacing;
        }
        
        int textBlockY = Padding + (_contentHeight - (_lines.Length * LineHeight)) / 2;
        for (int i = 0; i < _lines.Length; i++)
        {
            byte[] utf8Line = Encoding.UTF8.GetBytes(_lines[i]);
            int lineY = textBlockY + (i + 1) * LineHeight;
            XftDrawStringUtf8(draw, ref textColor, font, textStartX, lineY, utf8Line, utf8Line.Length);
        }
        
        int startX = (_width - _totalButtonWidth) / 2;
        int by = _height - Padding - ButtonHeight;

        _buttonBounds.Clear();
        _buttonResults.Clear();

        for (int i = 0; i < _buttonLabels.Length; i++)
        {
            int bx = startX + i * (ButtonWidth + ButtonSpacing);
            string label = "[" + _buttonLabels[i] + "]";
            byte[] utf8Label = Encoding.UTF8.GetBytes(label);

            var isFocused = (i == _focusedButton);
            var fgColor = isFocused ? invertColor : textColor;

            XftDrawRect(draw, isFocused ? textColor : bgColor, bx, by, ButtonWidth, ButtonHeight);
            if (isFocused)
            {
                IntPtr display = XftDrawDisplay(draw);
                IntPtr drawable = XftDrawDrawable(draw);
                IntPtr gc = XCreateGC(display, drawable, UIntPtr.Zero, IntPtr.Zero);
                XSetForeground(display, gc, fgColor.pixel);
                XDrawRectangle(display, drawable, gc, bx, by, (uint)ButtonWidth - 1, (uint)ButtonHeight - 1);
                XFreeGC(display, gc);
            }

            XftDrawStringUtf8(draw, ref fgColor, font, bx + 10, by + 20, utf8Label, utf8Label.Length);

            MessageBoxResult mapped = _buttonLabels[i] switch
            {
                var b when b == _cp.Ok => MessageBoxResult.Ok,
                var b when b == _cp.Cancela => MessageBoxResult.Cancel,
                var b when b == _cp.Sim => MessageBoxResult.Yes,
                var b when b == _cp.Nao => MessageBoxResult.No,
                _ => MessageBoxResult.Ok
            };

            if (mapped == MessageBoxResult.Cancel)
                _cancelResult = mapped;

            _buttonBounds[i] = (bx, bx + ButtonWidth, by, by + ButtonHeight);
            _buttonResults[i] = mapped;
        }

        XftFontClose(_display, font);
        XftDrawDestroy(draw);
        XFlush(_display);
    }

    [DllImport("libX11.so.6")] 
    private static extern void XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, uint width, uint height);
    
    [DllImport("libX11.so.6")] 
    private static extern IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, uint width, uint height, uint borderWidth, int depth, uint @class, IntPtr visual, ulong valuemask, ref XSetWindowAttributes attributes);
    
    [StructLayout(LayoutKind.Sequential)] 
    private struct XSetWindowAttributes { public int background_pixmap; public ulong background_pixel; public int border_pixmap; public ulong border_pixel; public int bit_gravity; public int win_gravity; public int backing_store; public ulong backing_planes; public ulong backing_pixel; public bool save_under; public ulong event_mask; public ulong do_not_propagate_mask; public bool override_redirect; public IntPtr colormap; public IntPtr cursor; }
    
    private static void XftDrawRect(IntPtr draw, XftColor color, int x, int y, int width, int height)
    {
        IntPtr display = XftDrawDisplay(draw);
        IntPtr drawable = XftDrawDrawable(draw);
        IntPtr gc = XCreateGC(display, drawable, UIntPtr.Zero, IntPtr.Zero);

        XSetForeground(display, gc, color.pixel);
        XFillRectangle(display, drawable, gc, x, y, (uint)width, (uint)height);

        XFreeGC(display, gc);
    }
    
    // Linux PInvoke X11
    // [StructLayout(LayoutKind.Sequential)]
    // private struct XGenericEvent
    // {
    //     public int type;
    //     public IntPtr serial;
    //     public bool send_event;
    //     public IntPtr display;
    //     public IntPtr window;
    // }

    [StructLayout(LayoutKind.Sequential)]
    private struct XKeyEvent
    {
        public int type;
        public IntPtr serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public IntPtr root;
        public IntPtr subwindow;
        public ulong time;
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public uint keycode;
        public bool same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XButtonEvent
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

    [StructLayout(LayoutKind.Sequential)]
    private struct XMotionEvent
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
        public byte is_hint;
        public bool same_screen;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    private struct XEvent
    {
        [FieldOffset(0)] public int type;
        [FieldOffset(0)] public XKeyEvent xkey;
        [FieldOffset(0)] public XButtonEvent xbutton;
        [FieldOffset(0)] public XMotionEvent xmotion;
    }
    
    [DllImport("libX11.so.6")]
    private static extern void XSetForeground(IntPtr display, IntPtr gc, ulong foreground);

    [DllImport("libX11.so.6")]
    private static extern void XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x, int y, uint width,
        uint height);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XCreateGC(IntPtr display, IntPtr drawable, UIntPtr valuemask, IntPtr values);

    [DllImport("libX11.so.6")]
    private static extern int XFreeGC(IntPtr display, IntPtr gc);

    [DllImport("libX11.so.6")]
    private static extern void XSync(IntPtr display, bool discard);

    [DllImport("libX11.so.6")]
    private static extern void XClearWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultColormap(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultVisual(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern void XFlush(IntPtr display);

    [DllImport("libX11.so.6")]
    public static extern void XUnloadFont(IntPtr display, IntPtr font);

    // [DllImport("libX11.so.6")]
    // private static extern void XSetFont(IntPtr display, IntPtr gc, IntPtr font);

    // [DllImport("libX11.so.6")]
    // private static extern IntPtr XLoadFont(IntPtr display, string fontName);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    // [DllImport("libX11.so.6")]
    // private static extern IntPtr XCreateSimpleWindow(
    //     IntPtr display, IntPtr parent,
    //     int x, int y, uint width, uint height,
    //     uint borderWidth, ulong border, ulong background);

    [DllImport("libX11.so.6")]
    private static extern void XMapRaised(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern void XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern void XStoreName(IntPtr display, IntPtr window, string windowName);

    [DllImport("libX11.so.6")]
    private static extern void XSelectInput(IntPtr display, IntPtr window, long eventMask);

    // [DllImport("libX11.so.6")]
    // private static extern void XDrawString(
    //     IntPtr display, IntPtr window, IntPtr gc,
    //     int x, int y, string str, int length);

    // [DllImport("libX11.so.6")]
    // private static extern IntPtr XDefaultGC(IntPtr display, int screenNumber);

    [DllImport("libX11.so.6")]
    private static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern void XNextEvent(IntPtr display, out XEvent xevent);

    // X11 Constants

    private const long KeyPressMask     = 0x00000001; // (1L << 0)
    private const long ButtonPressMask  = 0x00000004; // (1L << 2)
    private const long ExposureMask     = 0x00008000; // (1L << 15)
    private const long MotionNotifyMask = 0x00000040; // (1L << 6)
    
    [StructLayout(LayoutKind.Sequential)]
    private struct XftColor
    {
        public uint pixel;
        public ushort red, green, blue, alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XRenderColor
    {
        public ushort red, green, blue, alpha;
    }

    [DllImport("libXft.so.2")]
    private static extern IntPtr XftDrawDisplay(IntPtr draw);

    [DllImport("libXft.so.2")]
    private static extern IntPtr XftDrawDrawable(IntPtr draw);

    [DllImport("libXft.so.2")]
    private static extern IntPtr XftDrawCreate(IntPtr display, IntPtr drawable, IntPtr visual, IntPtr colormap);

    [DllImport("libXft.so.2")]
    private static extern void XftDrawDestroy(IntPtr draw);

    [DllImport("libXft.so.2")]
    private static extern IntPtr XftFontOpenName(IntPtr display, int screen,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    [DllImport("libXft.so.2")]
    private static extern void XftFontClose(IntPtr display, IntPtr font);

    [DllImport("libXft.so.2")]
    private static extern int XftColorAllocValue(IntPtr display, IntPtr visual, IntPtr colormap, ref XRenderColor color,
        out XftColor result);

    [DllImport("libXft.so.2")]
    private static extern void XftDrawStringUtf8(IntPtr draw, ref XftColor color, IntPtr font, int x, int y,
        byte[] text, int len);
}