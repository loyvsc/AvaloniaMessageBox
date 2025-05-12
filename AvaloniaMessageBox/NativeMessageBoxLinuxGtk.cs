using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CastelloBranco.AvaloniaMessageBox;

public static class NativeMessageBoxLinuxGtk
{
    // private static bool IsGtkAvailable()
    // {
    //     bool isGtkAvailable = false;
    //
    //     IntPtr gtk = IntPtr.Zero;
    //     
    //     try
    //     { 
    //         gtk = dlopen("libgtk-3.so.0", RTLD_NOW);
    //         
    //         if (gtk == IntPtr.Zero)
    //             return false;
    //         
    //         isGtkAvailable =
    //             dlsym(gtk, "gtk_init") != IntPtr.Zero &&
    //             dlsym(gtk, "gtk_message_dialog_new") != IntPtr.Zero &&
    //             dlsym(gtk, "gtk_dialog_run") != IntPtr.Zero &&
    //             dlsym(gtk, "gtk_widget_destroy") != IntPtr.Zero;
    //     }
    //     finally
    //     {
    //         if (gtk != IntPtr.Zero)
    //         {
    //             dlclose(gtk);
    //         }
    //     }
    //     
    //     return isGtkAvailable;
    // }
    //
    
    // private static MessageBoxResult ShowOnLinuxGtk (string title, string message, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
    // {
    //     IntPtr gtk = dlopen("libgtk-3.so.0", RTLD_NOW);
    //     
    //     if (gtk == IntPtr.Zero)
    //         throw new Exception("Failed to open libgtk-3.so.0");
    //
    //     try
    //     {
    //         var gtk_init = Marshal.GetDelegateForFunctionPointer<GtkInitDelegate>(dlsym(gtk, "gtk_init"));
    //         var gtk_message_dialog_new = Marshal.GetDelegateForFunctionPointer<GtkMessageDialogNewDelegate>(dlsym(gtk, "gtk_message_dialog_new"));
    //         var gtk_dialog_run = Marshal.GetDelegateForFunctionPointer<GtkDialogRunDelegate>(dlsym(gtk, "gtk_dialog_run"));
    //         var gtk_widget_destroy = Marshal.GetDelegateForFunctionPointer<GtkWidgetDestroyDelegate>(dlsym(gtk, "gtk_widget_destroy"));
    //         var gtk_widget_show_all = Marshal.GetDelegateForFunctionPointer<GtkWidgetShowAllDelegate>(dlsym(gtk, "gtk_widget_show_all"));
    //
    //         int argc = 0;
    //         IntPtr argv = IntPtr.Zero;
    //         gtk_init(ref argc, ref argv);
    //
    //         int gtkButtons = buttons switch
    //         {
    //             MessageBoxButtons.Ok => 1,        // GTK_BUTTONS_OK
    //             MessageBoxButtons.OkCancel => 5,  // GTK_BUTTONS_OK_CANCEL
    //             MessageBoxButtons.YesNo => 4,     // GTK_BUTTONS_YES_NO
    //             _ => 1
    //         };
    //
    //         int type =(int) (icon switch
    //         {
    //             MessageBoxIcon.None => MessageBoxGtkIcon.None,
    //             MessageBoxIcon.Error => MessageBoxGtkIcon.Error,
    //             MessageBoxIcon.Question => MessageBoxGtkIcon.Question,
    //             MessageBoxIcon.Information => MessageBoxGtkIcon.Info,
    //             MessageBoxIcon.Warning => MessageBoxGtkIcon.Warning,
    //             MessageBoxIcon.Stop    => MessageBoxGtkIcon.Stop,
    //             MessageBoxIcon.Success => MessageBoxGtkIcon.Success,
    //             _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, null)
    //         });
    //
    //         IntPtr dialog = gtk_message_dialog_new(IntPtr.Zero, 0, (int)type, gtkButtons, message, IntPtr.Zero);
    //         
    //         gtk_widget_show_all(dialog);
    //
    //         int response = gtk_dialog_run(dialog);
    //         
    //         gtk_widget_destroy(dialog);
    //
    //         return response switch
    //         {
    //             -5 => MessageBoxResult.Ok,     // GTK_RESPONSE_OK
    //             -6 => MessageBoxResult.Cancel, // GTK_RESPONSE_CANCEL
    //             -8 => MessageBoxResult.Yes,    // GTK_RESPONSE_YES
    //             -9 => MessageBoxResult.No,     // GTK_RESPONSE_NO
    //             _ => MessageBoxResult.None
    //         };
    //     }
    //     finally
    //     {
    //         dlclose(gtk);
    //     }
    // }
    //
    
    // private static Task<MessageBoxResult> ShowOnLinuxGtkAsync(string caption, string text,  MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
    // {
    //     return Task.Run(() => ShowOnLinuxGtk (caption, text, buttons, icon));
    // }
    //
    
    // =======================================================================
    // Linux PInvoke GTK Methotds 
    
    // private const int RTLD_NOW = 2;
    //
    // public enum MessageBoxGtkIcon
    // {
    //     None = -1,
    //     Info = 0,
    //     Warning = 1,
    //     Question = 2,
    //     Error = 3,
    //     Stop = 3,        // GTK does not have a specific "Stop", use GTK_MESSAGE_ERROR
    //     Success = 0     // GTK does not have a specific "Success", use GTK_MESSAGE_INFO
    // }
    //
    // [DllImport("libdl.so.2")]
    // private static extern IntPtr dlopen(string fileName, int flags);
    //
    // [DllImport("libdl.so.2")]
    // private static extern IntPtr dlerror();
    //
    // // Native P/Invoke
    //
    // [DllImport("libdl.so.2")]
    // private static extern IntPtr dlsym(IntPtr handle, string name);
    //
    // [DllImport("libdl.so.2")]
    // private static extern int dlclose(IntPtr handle);
    //
    // // GTK function delegates
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // private delegate void GtkInitDelegate(ref int argc, ref IntPtr argv);
    //
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // private delegate IntPtr GtkMessageDialogNewDelegate(IntPtr parent, int flags, int type, int buttons, string message, IntPtr args);
    //
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // private delegate int GtkDialogRunDelegate(IntPtr dialog);
    //
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // private delegate void GtkWidgetDestroyDelegate(IntPtr widget);
    //
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // private delegate void GtkWidgetShowAllDelegate(IntPtr widget);

}