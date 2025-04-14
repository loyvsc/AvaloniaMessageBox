using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
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

namespace CastelloBranco.MessageBox;

// OpenSource from Castello Branco Tecnologia => Github at 
// https://github.com/CastelloBrancoTecnologia/AvaloniaMessageBox
// MIT License

public static class AvaloniaWindowedMessageBox
{
    public static double MinAllowedWidth { get; set; } = 280;
    public static double MinAllowedHeight { get; set; } = 180;

    public enum MessageBoxButtons : uint { Ok = 0, OkCancel = 1, YesNo = 2 }
    public enum MessageBoxResult { Ok = 1, Cancel = 2, Yes = 3, No = 4 }
    public enum MessageBoxIcon { None, Information, Warning, Error, Question, Stop, Success }

    public static async Task<MessageBoxResult> ShowAsync(
        object? parent,
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        bool isDesktop = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime;
        bool isSingleView = Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime;

        Window? ownerWindow = parent as Window;
        UserControl? ownerControl = parent as UserControl;
        Window window = new Window();
        Rect screenBounds = (ownerWindow ?? ownerControl?.GetVisualRoot() as Window)?.Bounds
                            ?? window.Screens.Primary?.Bounds.ToRect(window.Screens.Primary?.Scaling ?? 1)
                            ?? new Rect(0, 0, 1920, 1080);       
        
        double maxAllowedWidth = screenBounds.Width * 0.8;
        Typeface typeface =  new Typeface(FontFamily.Default);
        var formattedTitle = new FormattedText(title, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 16, null);
        double titleWidth = formattedTitle.Width + 40;

        string[] lines = message.Split('\n');
        double maxLineWidth = 0;
        foreach (var line in lines)
        {
            var ft = new FormattedText(line, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 14, null);
            if (ft.Width > maxLineWidth)
                maxLineWidth = ft.Width;
        }

        double iconWidth = 50;
        double contentWidth = iconWidth + maxLineWidth + 80;
        double finalWidth = Math.Min(Math.Max(MinAllowedWidth, Math.Max(titleWidth, contentWidth)), maxAllowedWidth);

        var tcs = new TaskCompletionSource<MessageBoxResult>();
        Button? defaultButton = null;
        Button? cancelButton = null;
        
        Border titlePanel = new Border
        {
            [!Border.BackgroundProperty] = new DynamicResourceExtension("SystemControlBackgroundBaseLowBrush"),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Color = App.CurrentApp.ActualThemeVariant == ThemeVariant.Dark ? Color.FromArgb(0x88, 0, 0, 0) : Color.FromArgb(0x33, 0, 0, 0),
                Blur = 8,
                OffsetX = 0,
                OffsetY = 2
            }),
            Padding = new Thickness(0, 4, 0, 4),
            Child = new TextBlock
            {
                FontFamily = FontFamily.Default,
                Text = title,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = finalWidth - 40,
                Margin = new Thickness(10, 10, 10, 0),
                [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextForegroundBrush")
            }
        };
        
        titlePanel.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(titlePanel).Properties.IsLeftButtonPressed)
            {
                window.BeginMoveDrag(e);
            }
        };

        string iconText = icon switch
        {
            MessageBoxIcon.Information => "ℹ️",
            MessageBoxIcon.Warning => "⚠️",
            MessageBoxIcon.Error => "❌",
            MessageBoxIcon.Question => "❓",
            MessageBoxIcon.Stop => "🛑",
            MessageBoxIcon.Success => "✔",
            _ => string.Empty
        };

        
        Control iconBlock;

        if (icon == MessageBoxIcon.Stop)
        {
            // 🛑 com ✋ sobreposto
            iconBlock = new Grid
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(10, 0, 10, 0),
                Children =
                {
                    new TextBlock
                    {
                        Text = "🛑",
                        FontSize = 36,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontFamily = FontFamily.Default,
                        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextForegroundBrush")
                    },
                    new TextBlock
                    {
                        Text = "✋",
                        FontSize = 18,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontFamily = FontFamily.Default,
                        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextForegroundBrush")
                    }
                }
            };
        }
        else
        {
            // Outros ícones normais
            iconBlock = new TextBlock
            {
                FontFamily = FontFamily.Default,
                Text = iconText,
                FontSize = 36,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                Width = 50,
                Height = 50,
                TextAlignment = TextAlignment.Center,
                [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextForegroundBrush")
            };
        }

        TextBlock messageBlock = new TextBlock
        {
            FontFamily = FontFamily.Default,
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Justify,
            VerticalAlignment = VerticalAlignment.Top,
            MaxWidth = finalWidth - 80,
            Margin = new Thickness(0, 10, 10, 10)
        };

        StackPanel messagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 20, 10, 0),
            Children = { iconBlock, messageBlock }
        };

        StackPanel buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 10)
        };

        void AddButton(string content, MessageBoxResult result, bool isDefault = false, bool isCancel = false)
        {
            TextBlock textBlock = new TextBlock();
    
            if (content.StartsWith("✔"))
            {
                textBlock.Inlines!.Add(new Run("✔") { Foreground = Brushes.Green, FontWeight = FontWeight.Bold });
                textBlock.Inlines!.Add(new Run(content.Substring(1)));
            }
            else if (content.StartsWith("❌"))
            {
                textBlock.Inlines!.Add(new Run("❌") { Foreground = Brushes.Red });
                textBlock.Inlines!.Add(new Run(content.Substring(1)));
            }
            else
            {
                textBlock.Text = content;
            }
            
            var button = new Button
            {
                Content = textBlock,
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 75
            };
            button.Click += (_, _) => { tcs.TrySetResult(result); window.Close(); };
            if (isDefault) defaultButton = button;
            if (isCancel) cancelButton = button;
            buttonPanel.Children.Add(button);
        }


        CulturePrompt? cp = AllCultures.SingleOrDefault(x => CultureInfo.CurrentCulture.Name.StartsWith(x.Culture)) ??
                            AllCultures[0];

        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                AddButton($"✔ {cp.Ok}", MessageBoxResult.Ok, isDefault: true);
                break;
            case MessageBoxButtons.OkCancel:
                AddButton($"✔ {cp.Ok}", MessageBoxResult.Ok, isDefault: true);
                AddButton($"❌ {cp.Cancela}", MessageBoxResult.Cancel, isCancel: true);
                break;
            case MessageBoxButtons.YesNo:
                AddButton($"✔ {cp.Sim}", MessageBoxResult.Yes, isDefault: true);
                AddButton($"❌ {cp.Nao}", MessageBoxResult.No, isCancel: true);
                break;
        }

        int lineCount = Math.Max(lines.Length, message.Length / 60 + 1);
        double lineHeight = 22;
        double estimatedTextHeight = lineCount * lineHeight;
        double baseHeight = MinAllowedHeight;
        double estimatedTotalHeight = baseHeight + Math.Max(0, estimatedTextHeight - lineHeight);

        StackPanel stackPanel = new StackPanel
        {
            Children = { titlePanel, messagePanel, buttonPanel }
        };

        ScrollViewer scrollableContent = new ScrollViewer
        {
            Content = stackPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(0)
        };
        
        // configure already created window
        window.Width = finalWidth;
        window.Height = Math.Min(estimatedTotalHeight, screenBounds.Height * 0.9);
        window.CanResize = false;
        window.ShowInTaskbar = false;
        window.SystemDecorations = SystemDecorations.BorderOnly;
        window.Icon = ownerWindow?.Icon;
        window.WindowState = WindowState.Normal;
        window.Topmost = true;
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.Content = scrollableContent;
        
        window.Opened += (_, _) =>
        {
            NSApplicationHelper.SetWindowLevel(window, NSApplicationHelper.NSNotificationWindowLevel);
            defaultButton?.Focus();
        };

        window.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && defaultButton is not null)
            {
                defaultButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && cancelButton is not null)
            {
                cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
        };

        if (Application.Current?.ApplicationLifetime is null)
        {
            window.Show();
        }
        else if (isDesktop && ownerWindow is not null)
        {
            await window.ShowDialog(ownerWindow);
        }
        else if (isSingleView && 
                 ownerControl?.Content is Panel panel)
        {
            panel.Children.Add(window);
            ownerControl.IsEnabled = false;
            window.Closed += (_, _) => { ownerControl.IsEnabled = true; panel.Children.Remove(window); };
            window.Show();
        }
        else
        {
            window.Show();
        }

        return await tcs.Task;
    }

    public class CulturePrompt (string culture, string sim, string nao, string ok, string cancela)
    {
        public string Culture { get; set; } = culture;
        public string Sim { get; set; } = sim;
        public string Nao { get; set; } = nao;
        public string Ok  { get; set; } = ok;
        public string Cancela { get; set; } = cancela;
    }

    public static CulturePrompt[] AllCultures { get; } =
    [
        // Globais e asiáticas
        new("en", "Yes", "No", "OK", "Cancel"),
        new("zh-CN", "是", "否", "好的", "取消"),
        new("ja", "はい", "いいえ", "OK", "キャンセル"),
        new("hi-IN", "हाँ", "नहीं", "ठीक है", "रद्द करें"),
        new("ru", "Да", "Нет", "ОК", "Отмена"),
        new("es", "Sí", "No", "OK", "Cancelar"),
        new("fr", "Oui", "Non", "OK", "Annuler"),
        new("ar", "نعم", "لا", "موافق", "إلغاء"),
        new("pt-BR", "Sim", "Não", "OK", "Cancelar"),
        new("de", "Ja", "Nein", "OK", "Abbrechen"),
        new("it", "Sì", "No", "OK", "Annulla"),
        new("ko", "예", "아니요", "확인", "취소"),
        new("tr", "Evet", "Hayır", "Tamam", "İptal"),
        new("fa", "بله", "خیر", "باشه", "لغو"),
        new("vi", "Có", "Không", "OK", "Hủy"),
        new("id", "Ya", "Tidak", "OK", "Batal"),
        new("th", "ใช่", "ไม่ใช่", "ตกลง", "ยกเลิก"),

        // Europeias — oficiais e regionais
        new("pt-PT", "Sim", "Não", "OK", "Cancelar"),
        new("pl", "Tak", "Nie", "OK", "Anuluj"),
        new("uk", "Так", "Ні", "OK", "Скасувати"),
        new("ro", "Da", "Nu", "OK", "Anulează"),
        new("nl", "Ja", "Nee", "OK", "Annuleren"),
        new("sv", "Ja", "Nej", "OK", "Avbryt"),
        new("no", "Ja", "Nei", "OK", "Avbryt"),
        new("fi", "Kyllä", "Ei", "OK", "Peruuta"),
        new("da", "Ja", "Nej", "OK", "Annuller"),
        new("cs", "Ano", "Ne", "OK", "Zrušit"),
        new("sk", "Áno", "Nie", "OK", "Zrušiť"),
        new("hu", "Igen", "Nem", "OK", "Mégse"),
        new("el", "Ναι", "Όχι", "OK", "Ακύρωση"),
        new("lt", "Taip", "Ne", "Gerai", "Atšaukti"),
        new("lv", "Jā", "Nē", "Labi", "Atcelt"),
        new("et", "Jah", "Ei", "OK", "Tühista"),
        new("sl", "Da", "Ne", "V redu", "Prekliči"),
        new("hr", "Da", "Ne", "U redu", "Odustani"),
        new("sr", "Да", "Не", "У реду", "Откажи"),
        new("bg", "Да", "Не", "Добре", "Отказ"),
        new("mk", "Да", "Не", "Во ред", "Откажи"),
        new("sq", "Po", "Jo", "OK", "Anulo"),
        new("bs", "Da", "Ne", "U redu", "Otkaži"),
        new("is", "Já", "Nei", "Í lagi", "Hætta við"),
        new("af", "Ja", "Nee", "OK", "Kanselleer"),
        new("he", "כן", "לא", "אישור", "ביטול"),
        new("ga", "Tá", "Níl", "OK", "Cealaigh"),
        new("cy", "Ie", "Na", "Iawn", "Canslo"),
        new("gd", "Tha", "Chan eil", "Ceart ma-thà", "Sguir dheth"),
        new("br", "Ya", "Ket", "Mat eo", "Nullañ"),
        new("co", "Iè", "Innò", "OK", "Abbandunà"),
        new("rm", "Gea", "Na", "OK", "Annulà"),
        new("fur", "Sì", "No", "Va ben", "Anule"),
        new("lad", "Sí", "No", "OK", "Anular"),
        new("sc", "Iè", "Non", "OK", "Cancella"),
        new("eu", "Bai", "Ez", "Ados", "Utzi"),
        new("ca", "Sí", "No", "D'acord", "Cancel·la"),
        new("wa", "Oyi", "Neni", "Dacor", "Rinoncî"),
        new("be", "Так", "Не", "ОК", "Адмяніць"),
        new("hy", "Այո", "Ոչ", "Լավ", "Չեղարկել"),
        new("ka", "დიახ", "არა", "კარგი", "გაუქმება"),
        new("lb", "Jo", "Nee", "OK", "Ofbriechen"),
        new("mwl", "Sim", "Nun", "OK", "Cancelear"),

        // Africanas principais
        new("sw", "Ndiyo", "Hapana", "Sawa", "Ghairi"),
        new("zu", "Yebo", "Cha", "Kulungile", "Khansela"),
        new("xh", "Ewe", "Hayi", "Kulungile", "Rhoxisa"),
        new("yo", "Bẹẹni", "Rárá", "O Dára", "Fagile"),
        new("ig", "Ee", "Mba", "Ọ Dịrị Mma", "Kagbuo"),
        new("ha", "I", "A'a", "To", "Soke"),
        new("am", "አዎን", "አይ", "እሺ", "ይቅር"),
        new("so", "Haa", "Maya", "Haa", "Ka noqo"),
        new("ti", "እወ", "ኣይ", "እሺ", "ተወው"),
        new("ff", "Eey", "Alaa", "Waaw", "Dagg"),
        new("wo", "Waaw", "Déedéet", "OK", "Neenal"),
        new("ln", "Ee", "Te", "Malamu", "Koboya"),
        new("mg", "Eny", "Tsia", "OK", "Foano"),
        new("ak", "Aane", "Daabi", "Yoo", "Twa"),
        new("st", "E", "Tjhe", "Ho Lokile", "Hlakholla"),
        new("tn", "Ee", "Nnyaa", "Go Lokile", "Khansela"),
        new("sn", "Hongu", "Kwete", "Zvakanaka", "Kanzura")
    ];

}
