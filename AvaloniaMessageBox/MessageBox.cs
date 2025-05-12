using System.Globalization;
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

public static class MessageBox
{
    public static double MinAllowedWidth { get; set; } = 280;
    public static double MinAllowedHeight { get; set; } = 180;
    
    public static async Task<MessageBoxResult> ShowAsync(
        object? parent,
        string caption,
        string text,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (IsAvaloniaReady())
        {
            return await ShowOnAvaloniaAsync(parent, caption, text, buttons, icon);
        }
        else
        {
            return await NativeOsMessageBox.ShowAsync(parent, caption, text, buttons, icon);
        }
    }

    public static async Task<MessageBoxResult> ShowAsync(
        object? parent,
        string text,
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (IsAvaloniaReady())
        {
            return await ShowOnAvaloniaAsync(parent, string.Empty, text, buttons, icon);
        }
        else
        {
            return await NativeOsMessageBox.ShowAsync(parent, String.Empty, text, buttons, icon);
        }
    }
    
    private static bool IsAvaloniaReady()
    {
        if (Application.Current is null)
            return false;

        if (Application.Current.PlatformSettings is null)
            return false;

        return Application.Current.ApplicationLifetime != null;
    }
    
    private static async Task<MessageBoxResult> ShowOnAvaloniaAsync(
        object? parent,
        string caption,
        string text,
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
        Typeface typeface = new Typeface(FontFamily.Default);
        
        var formattedTitle = new FormattedText(caption, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 16, null);
        double titleWidth = formattedTitle.Width + 40;

        string[] lines = text.Split('\n');
        double maxLineWidth = 0;
        foreach (var line in lines)
        {
            var ft = new FormattedText(line, (CultureInfo.CurrentCulture?.Name?.Length > 0 ? CultureInfo.CurrentCulture : new CultureInfo("en")), FlowDirection.LeftToRight, typeface, 14, null);
            if (ft.Width > maxLineWidth)
                maxLineWidth = ft.Width;
        }

        double iconWidth = 50;
        double contentWidth = iconWidth + maxLineWidth + 80;
        double finalWidth = Math.Min(Math.Max(MinAllowedWidth, Math.Max(titleWidth, contentWidth)), maxAllowedWidth);

        TaskCompletionSource<MessageBoxResult> tcs = new();

        Button? defaultButton = null;
        Button? cancelButton = null;

        Border titlePanel = new Border
        {
            [!Border.BackgroundProperty] = new DynamicResourceExtension("SystemControlBackgroundBaseLowBrush"),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Color = Application.Current!.ActualThemeVariant == ThemeVariant.Dark
                    ? Color.FromArgb(0x88, 0, 0, 0)
                    : Color.FromArgb(0x33, 0, 0, 0),
                Blur = 8,
                OffsetX = 0,
                OffsetY = 2
            }),
            Padding = new Thickness(0, 4, 0, 4),
            Child = new TextBlock
            {
                FontFamily = FontFamily.Default,
                Text = caption,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = finalWidth - 40,
                Margin = new Thickness(10, 10, 10, 0),
                [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextForegroundBrush")
            }
        };

        titlePanel.PointerPressed += (_, e) =>
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
            // 🛑 com ✋ sobreposto (⚠️ pode não aparecer corretamente no X11 sem fonte Unicode)
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
            Text = text,
            FlowDirection = CultureInfo.CurrentCulture!.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Justify,
            VerticalAlignment = VerticalAlignment.Top,
            MaxWidth = finalWidth - 80,
            Margin = new Thickness(0, 10, 10, 10)
        };
        
        AutomationProperties.SetName(messageBlock, text);

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
            
            AutomationProperties.SetName(button, content);
            
            button.Click += (_, _) =>
            {
                tcs.TrySetResult(result);
                window.Close();
            };
            if (isDefault) defaultButton = button;
            if (isCancel) cancelButton = button;
            buttonPanel.Children.Add(button);
        }

        CulturePrompt cp = CulturePrompt.Current;

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

        int lineCount = Math.Max(lines.Length, text.Length / 60 + 1);
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

        if (isDesktop && ownerWindow is not null)
        {
            await window.ShowDialog(ownerWindow);
        }
        else if (isSingleView && ownerWindow is not null)
        {
            // Detecta visual root com children (ex: StackPanel, Grid, DockPanel)
            Panel? hostPanel = ownerWindow.GetVisualDescendants()
                .OfType<Panel>()
                .FirstOrDefault();

            if (hostPanel is not null)
            {
                hostPanel.Children.Add(window);
                ownerWindow.IsEnabled = false;

                window.Closed += (_, _) =>
                {
                    ownerWindow.IsEnabled = true;
                    hostPanel.Children.Remove(window);
                };
            }

            window.Show();
        }
        else
        {
            window.Show();
        }

        return await tcs.Task;
    }
}


