using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
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

    private class CulturePrompt (
        string culture,
        string sim,
        string nao,
        string ok,
        string cancela,
        string exception,
        string anErrorWasOcurred,
        string location,
        string message)
    {
        public string Culture { get; set; } = culture;
        public string Sim { get; set; } = sim;
        public string Nao { get; set; } = nao;
        public string Ok  { get; set; } = ok;
        public string Cancela { get; set; } = cancela;
        public string Exception { get; set; } = exception;
        public string AnErrorWasOcurred { get; set; } = anErrorWasOcurred;
        public string Location { get; set; } = location;
        public string Message { get; set; } = message;
    }
    
    public static async Task ShowExceptionDialogAsync(object? parent, Exception ex)
    {
        string exceptionName = ex.GetType().Name;
        string exceptionMessage = ex.Message;

        string? fileName = null;
        int? lineNumber = null;

        try
        {
            var st = new StackTrace(ex, true);
            var firstFrame = st.GetFrames()?.FirstOrDefault(f =>
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

        var cp = AllCultures.FirstOrDefault(c => CultureInfo.CurrentCulture.Name.StartsWith(c.Culture)) 
                 ?? AllCultures[0];
        
        var sb = new StringBuilder();
        sb.AppendLine($"{cp.Exception}: {exceptionName}");
        if (!string.IsNullOrWhiteSpace(fileName) && lineNumber.HasValue)
            sb.AppendLine($"{cp.Location}: {System.IO.Path.GetFileName(fileName)}:{lineNumber}");
        sb.AppendLine($"{cp.Message}: {exceptionMessage}");

        await AvaloniaWindowedMessageBox.ShowAsync(
            parent,
            cp.AnErrorWasOcurred,
            sb.ToString().Trim(),
            AvaloniaWindowedMessageBox.MessageBoxButtons.Ok,
            AvaloniaWindowedMessageBox.MessageBoxIcon.Stop
        );
    }

    private static CulturePrompt[] AllCultures =
    [
        new("en",        "Yes",       "No",      "Ok",      "Cancel",    "Exception",       "An error occurred",   "Location", "Message"),
        new("pt",        "Sim",       "Não",     "Ok",      "Cancelar",  "Exceção",         "Ocorreu um erro",      "Localização", "Mensagem"),
        new("es",        "Sí",        "No",      "Aceptar", "Cancelar",  "Excepción",       "Ocurrió un error",     "Ubicación", "Mensaje"),
        new("fr",        "Oui",       "Non",     "Ok",      "Annuler",   "Exception",       "Une erreur est survenue", "Emplacement", "Message"),
        new("de",        "Ja",        "Nein",    "Ok",      "Abbrechen", "Ausnahme",        "Ein Fehler ist aufgetreten", "Speicherort", "Nachricht"),
        new("it",        "Sì",        "No",      "Ok",      "Annulla",   "Eccezione",       "Si è verificato un errore", "Posizione", "Messaggio"),
        new("ja",        "はい",      "いいえ",    "OK",      "キャンセル", "例外",          "エラーが発生しました",    "場所", "メッセージ"),
        new("zh-CN",     "是",        "否",      "确定",    "取消",      "异常",          "发生了一个错误",      "位置", "消息"),
        new("ru",        "Да",        "Нет",     "ОК",      "Отмена",    "Исключение",      "Произошла ошибка",     "Местоположение", "Сообщение"),
        new("ko",        "예",        "아니오",    "확인",    "취소",      "예외",          "오류가 발생했습니다",    "위치", "메시지"),
        new("ar",        "نعم",       "لا",       "موافق",    "إلغاء",     "استثناء",        "حدث خطأ",             "الموقع", "الرسالة"),
        new("en-US",     "Yes",       "No",      "OK",      "Cancel",    "Exception",       "An error occurred",   "Location", "Message"),
        new("en-GB",     "Yes",       "No",      "OK",      "Cancel",    "Exception",       "An error occurred",   "Location", "Message"),
        new("es-ES",     "Sí",        "No",      "Aceptar", "Cancelar",  "Excepción",       "Ha ocurrido un error",  "Ubicación", "Mensaje"),
        new("fr-FR",     "Oui",       "Non",     "OK",      "Annuler",   "Exception",       "Une erreur s'est produite", "Emplacement", "Message"),
        new("de-DE",     "Ja",        "Nein",    "OK",      "Abbrechen", "Ausnahme",        "Es ist ein Fehler aufgetreten", "Speicherort", "Nachricht"),
        new("it-IT",     "Sì",        "No",      "OK",      "Annulla",   "Eccezione",       "Si è verificato un errore", "Posizione", "Messaggio"),
        new("ja-JP",     "はい",      "いいえ",    "OK",      "キャンセル", "例外",          "エラーが発生しました",    "場所", "メッセージ"),
        new("zh-Hans",   "是",        "否",      "确定",    "取消",      "异常",          "发生了一个错误",      "位置", "消息"),
        new("ru-RU",     "Да",        "Нет",     "ОК",      "Отмена",    "Исключение",      "Произошла ошибка",     "Местоположение", "Сообщение"),
        new("ko-KR",     "예",        "아니오",    "확인",    "취소",      "예외",          "오류가 발생했습니다",    "위치", "메시지"),
        new("ar-SA",     "نعم",       "لا",       "موافق",    "إلغاء",     "استثناء",        "حدث خطأ",             "الموقع", "الرسالة"),
        new("nl",        "Ja",        "Nee",     "Ok",      "Annuleren", "Uitzondering",    "Er is een fout opgetreden", "Locatie", "Bericht"),
        new("sv",        "Ja",        "Nej",     "Ok",      "Avbryt",    "Undantag",        "Ett fel har inträffat", "Plats", "Meddelande"),
        new("no",        "Ja",        "Nei",     "Ok",      "Avbryt",    "Unntak",        "Det har oppstått en feil", "Plassering", "Melding"),
        new("da",        "Ja",        "Nej",     "Ok",      "Annuller",  "Undtagelse",      "Der er opstået en fejl", "Placering", "Besked"),
        new("fi",        "Kyllä",     "Ei",      "Ok",      "Peruuta",   "Poikkeus",        "Tapahtui virhe",       "Sijainti", "Viesti"),
        new("pl",        "Tak",       "Nie",     "Ok",      "Anuluj",    "Wyjątek",         "Wystąpił błąd",         "Lokalizacja", "Wiadomość"),
        new("cs",        "Ano",       "Ne",      "Ok",      "Zrušit",    "Výjimka",         "Došlo k chybě",        "Umístění", "Zpráva"),
        new("hu",        "Igen",      "Nem",     "Ok",      "Mégse",     "Kivétel",         "Hiba történt",         "Hely", "Üzenet"),
        new("tr",        "Evet",      "Hayır",    "Tamam",   "İptal",     "İstisna",         "Bir hata oluştu",      "Konum", "Mesaj"),
        new("el",        "Ναι",       "Όχι",      "Εντάξει",  "Άκυρο",     "Εξαίρεση",        "Παρουσιάστηκε σφάλμα",   "Τοποθεσία", "Μήνυμα"),
        new("he",        "כן",        "לא",       "אישור",    "ביטול",     "חריגה",          "אירעה שגיאה",        "מיקום", "הודעה"),
        new("id",        "Ya",        "Tidak",   "Oke",     "Batal",     "Pengecualian",    "Terjadi kesalahan",    "Lokasi", "Pesan"),
        new("vi",        "Có",        "Không",   "OK",      "Hủy bỏ",    "Ngoại lệ",        "Đã xảy ra lỗi",       "Vị trí", "Tin nhắn"),
        new("th",        "ใช่",       "ไม่ใช่",    "ตกลง",    "ยกเลิก",    "ข้อยกเว้น",       "เกิดข้อผิดพลาด",      "ตำแหน่ง", "ข้อความ"),
        new("uk",        "Так",       "Ні",      "OK",      "Скасувати",  "Виняток",         "Сталася помилка",      "Розташування", "Повідомлення"),
        new("ro",        "Da",        "Nu",      "OK",      "Anulează",  "Excepție",        "A apărut o eroare",     "Locație", "Mesaj"),
        new("sk",        "Áno",       "Nie",     "OK",      "Zrušiť",    "Výnimka",         "Vyskytla sa chyba",    "Umiestnenie", "Správa"),
        new("sl",        "Da",        "Ne",      "V redu",  "Prekliči",  "Izjema",          "Prišlo je do napake",   "Lokacija", "Sporočilo"),
        new("bg",        "Да",        "Не",      "ОК",      "Отказ",     "Изключение",      "Възникна грешка",      "Местоположение", "Съобщение"),
        new("hr",        "Da",        "Ne",      "U redu",  "Odustani",  "Iznimka",         "Došlo je do pogreške",  "Lokacija", "Poruka"),
        new("sr",        "Да",        "Не",      "У реду",  "Откажи",    "Изузетак",        "Дошло је до грешке",    "Локација", "Порука"),
        new("lt",        "Taip",      "Ne",      "Gerai",   "Atšaukti",  "Išimtis",         "Įvyko klaida",         "Vieta", "Pranešimas"),
        new("lv",        "Jā",        "Nē",      "Labi",    "Atcelt",    "Izņēmums",        "Ir notikusi kļūda",    "Atrašanās vieta", "Ziņojums"),
        new("et",        "Jah",       "Ei",      "OK",      "Tühista",   "Erand",           "Tekkis viga",          "Asukoht", "Sõnum"),
        new("mk",        "Да",        "Не",      "Во ред",  "Откажи",    "Исклучок",        "Настана грешка",       "Локација", "Порака"),
        new("sq",        "Po",        "Jo",      "OK",      "Anulo",     "Përjashtim",      "Ndodhi një gabim",     "Vendndodhja", "Mesazh"),
        new("az",        "Bəli",      "Xeyr",    "OK",      "Ləğv et",  "İstisna",         "Xəta baş verdi",        "Yer", "Mesaj"),
        new("bn",        "হ্যাঁ",       "না",       "ঠিক আছে",   "বাতিল করুন",  "ব্যতিক্রম",       "একটি ত্রুটি ঘটেছে",     "অবস্থান", "বার্তা"),
        new("fa",        "بله",       "نه",       "تایید",    "لغو",       "استثنا",         "خطایی رخ داده است",    "مکان", "پیام"),
        new("hi",        "हाँ",        "नहीं",     "ठीक है",    "रद्द करें",   "अपवाद",          "एक त्रुटि हुई",        "स्थान", "संदेश"),
        new("ka",        "დიახ",      "არა",      "კარგი",    "გაუქმება",  "გამონაკლისი",     "მოხდა შეცდომა",      "ადგილმდებარეობა", "შეტყობინება"),
        new("kk",        "Иә",        "Жоқ",      "Жарайды",  "Бас тарту",  "Айырып алу",      "Қате орын алды",      "Орналасқан жері", "Хабарлама"),
        new("km",        "បាទ/ចាស",   "ទេ",       "យល់ព្រម",   "បោះបង់",    "ករណីលើកលែង",    "មានកំហុសបានកើតឡើង", "ទីតាំង", "សារ"),
        new("ky",        "Ооба",      "Жок",      "Макул",    "Баш тарт",  "Айрыкча учур",    "Ката кетти",          "Жайгашкан жери", "Кабар"),
        new("lo",        "ແມ່ນ",       "ບໍ່",       "ຕົກລົງ",    "ຍົກເລີກ",    "ຂໍ້ຍົກເວັ້ນ",      "ເກີດຂໍ້ຜິດພາດ",      "ສະຖານທີ່", "ຂໍ້ຄວາມ"),
        new("mn",        "Тийм",      "Үгүй",     "OK",      "Цуцлах",    "Онцгой тохиолдол", "Алдаа гарлаа",        "Байршил", "Зурвас"),
        new("my",        "ဟုတ်ကဲ့",    "မဟုတ်ပါ",   "အိုကေ",    "ပယ်ဖျက်",    "ချွင်းချက်",       "အမှားတစ်ခုဖြစ်ပွားခဲ့သည်", "တည်နေရာ", "သတင်းစကား"),
        new("ne",        "हुन्छ",       "हुँदैन",    "ठीक छ",    "रद्द गर्नुहोस्", "अपवाद",          "त्रुटि भयो",          "स्थान", "सन्देश"),
        new("pa",        "ਹਾਂ",        "ਨਹੀਂ",     "ਠੀਕ ਹੈ",    "ਰੱਦ ਕਰੋ",    "ਅਪਵਾਦ",          "ਇੱਕ ਗਲਤੀ ਆਈ ਹੈ",     "ਸਥਾਨ", "ਸੁਨੇਹਾ"),
        new("si",        "ඔව්",        "නැත",     "හරි",      "අවලංගු කරන්න", "ව්යතිරේකය",      "දෝෂයක් සිදුවී ඇත",   "ස්ථානය", "පණිවිඩය"),
        new("sw",        "Ndiyo",      "Hapana",   "Sawa",    "Ghairi",    "Tofauti",        "Hitilafu imetokea",   "Mahali", "Ujumbe"),
        new("ta",        "ஆம்",        "இல்லை",    "சரி",      "ரத்து செய்",   "விலக்கு",         "ஒரு பிழை ஏற்பட்டது",    "இடம்", "செய்தி"),
        new("te",        "అవును",      "కాదు",     "సరే",      "రద్దు చేయి",   "మినహాయింపు",      "ఒక లోపం సంభవించింది",  "స్థానం", "సందేశం"),
        new("ur",        "جی ہاں",      "نہیں",     "ٹھیک ہے",    "منسوخ کریں",  "مستثنیٰ",        "ایک خرابی پیش آئی",   "مقام", "پیغام"),
        new("uz",        "Ha",        "Yo'q",     "OK",      "Bekor qilish", "Istisno",         "Xatolik yuz berdi",    "Manzil", "Xabar"),
        new("zh-TW",     "是",        "否",      "確定",    "取消",      "例外",          "發生了一個錯誤",      "位置", "訊息")
    ];
}
