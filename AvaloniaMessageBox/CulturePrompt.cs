
using System.Globalization;

namespace CastelloBranco.AvaloniaMessageBox;

   // ============================================================================================
    // International translation support

    internal class CulturePrompt(
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
        public string Ok { get; set; } = ok;
        public string Cancela { get; set; } = cancela;
        public string Exception { get; set; } = exception;
        public string AnErrorWasOcurred { get; set; } = anErrorWasOcurred;
        public string Location { get; set; } = location;
        public string Message { get; set; } = message;

        public static CulturePrompt Current
        {
            get
            {
                return AllCultures.SingleOrDefault(x => CultureInfo.CurrentCulture.Name.StartsWith(x.Culture)) ??
                                    AllCultures[0];
            }
        }
        
        private static readonly CulturePrompt[] AllCultures =
        [
            new("en", "Yes", "No", "Ok", "Cancel", "Exception", "An error occurred", "Location", "Message"),
            new("pt", "Sim", "Não", "Ok", "Cancelar", "Exceção", "Ocorreu um erro", "Localização", "Mensagem"),
            new("es", "Sí", "No", "Aceptar", "Cancelar", "Excepción", "Ocurrió un error", "Ubicación", "Mensaje"),
            new("fr", "Oui", "Non", "Ok", "Annuler", "Exception", "Une erreur est survenue", "Emplacement", "Message"),
            new("de", "Ja", "Nein", "Ok", "Abbrechen", "Ausnahme", "Ein Fehler ist aufgetreten", "Speicherort",
                "Nachricht"),
            new("it", "Sì", "No", "Ok", "Annulla", "Eccezione", "Si è verificato un errore", "Posizione", "Messaggio"),
            new("ja", "はい", "いいえ", "OK", "キャンセル", "例外", "エラーが発生しました", "場所", "メッセージ"),
            new("zh-CN", "是", "否", "确定", "取消", "异常", "发生了一个错误", "位置", "消息"),
            new("ko", "예", "아니오", "확인", "취소", "예외", "오류가 발생했습니다", "위치", "메시지"),
            new("ar", "نعم", "لا", "موافق", "إلغاء", "استثناء", "حدث خطأ", "الموقع", "الرسالة"),
            new("en-US", "Yes", "No", "OK", "Cancel", "Exception", "An error occurred", "Location", "Message"),
            new("en-GB", "Yes", "No", "OK", "Cancel", "Exception", "An error occurred", "Location", "Message"),
            new("es-ES", "Sí", "No", "Aceptar", "Cancelar", "Excepción", "Ha ocurrido un error", "Ubicación", "Mensaje"),
            new("fr-FR", "Oui", "Non", "OK", "Annuler", "Exception", "Une erreur s'est produite", "Emplacement", "Message"),
            new("de-DE", "Ja", "Nein", "OK", "Abbrechen", "Ausnahme", "Es ist ein Fehler aufgetreten", "Speicherort",
                "Nachricht"),
            new("it-IT", "Sì", "No", "OK", "Annulla", "Eccezione", "Si è verificato un errore", "Posizione", "Messaggio"),
            new("ja-JP", "はい", "いいえ", "OK", "キャンセル", "例外", "エラーが発生しました", "場所", "メッセージ"),
            new("zh-Hans", "是", "否", "确定", "取消", "异常", "发生了一个错误", "位置", "消息"),
            new("ru-RU", "Да", "Нет", "ОК", "Отмена", "Исключение", "Произошла ошибка", "Местоположение", "Сообщение"),
            new("ko-KR", "예", "아니오", "확인", "취소", "예외", "오류가 발생했습니다", "위치", "메시지"),
            new("ar-SA", "نعم", "لا", "موافق", "إلغاء", "استثناء", "حدث خطأ", "الموقع", "الرسالة"),
            new("nl", "Ja", "Nee", "Ok", "Annuleren", "Uitzondering", "Er is een fout opgetreden", "Locatie", "Bericht"),
            new("sv", "Ja", "Nej", "Ok", "Avbryt", "Undantag", "Ett fel har inträffat", "Plats", "Meddelande"),
            new("no", "Ja", "Nei", "Ok", "Avbryt", "Unntak", "Det har oppstått en feil", "Plassering", "Melding"),
            new("da", "Ja", "Nej", "Ok", "Annuller", "Undtagelse", "Der er opstået en fejl", "Placering", "Besked"),
            new("fi", "Kyllä", "Ei", "Ok", "Peruuta", "Poikkeus", "Tapahtui virhe", "Sijainti", "Viesti"),
            new("pl", "Tak", "Nie", "Ok", "Anuluj", "Wyjątek", "Wystąpił błąd", "Lokalizacja", "Wiadomość"),
            new("cs", "Ano", "Ne", "Ok", "Zrušit", "Výjimka", "Došlo k chybě", "Umístění", "Zpráva"),
            new("hu", "Igen", "Nem", "Ok", "Mégse", "Kivétel", "Hiba történt", "Hely", "Üzenet"),
            new("tr", "Evet", "Hayır", "Tamam", "İptal", "İstisna", "Bir hata oluştu", "Konum", "Mesaj"),
            new("el", "Ναι", "Όχι", "Εντάξει", "Άκυρο", "Εξαίρεση", "Παρουσιάστηκε σφάλμα", "Τοποθεσία", "Μήνυμα"),
            new("he", "כן", "לא", "אישור", "ביטול", "חריגה", "אירעה שגיאה", "מיקום", "הודעה"),
            new("id", "Ya", "Tidak", "Oke", "Batal", "Pengecualian", "Terjadi kesalahan", "Lokasi", "Pesan"),
            new("vi", "Có", "Không", "OK", "Hủy bỏ", "Ngoại lệ", "Đã xảy ra lỗi", "Vị trí", "Tin nhắn"),
            new("th", "ใช่", "ไม่ใช่", "ตกลง", "ยกเลิก", "ข้อยกเว้น", "เกิดข้อผิดพลาด", "ตำแหน่ง", "ข้อความ"),
            new("uk", "Так", "Ні", "OK", "Скасувати", "Виняток", "Сталася помилка", "Розташування", "Повідомлення"),
            new("ro", "Da", "Nu", "OK", "Anulează", "Excepție", "A apărut o eroare", "Locație", "Mesaj"),
            new("sk", "Áno", "Nie", "OK", "Zrušiť", "Výnimka", "Vyskytla sa chyba", "Umiestnenie", "Správa"),
            new("sl", "Da", "Ne", "V redu", "Prekliči", "Izjema", "Prišlo je do napake", "Lokacija", "Sporočilo"),
            new("bg", "Да", "Не", "ОК", "Отказ", "Изключение", "Възникна грешка", "Местоположение", "Съобщение"),
            new("hr", "Da", "Ne", "U redu", "Odustani", "Iznimka", "Došlo je do pogreške", "Lokacija", "Poruka"),
            new("sr", "Да", "Не", "У реду", "Откажи", "Изузетак", "Дошло је до грешке", "Локација", "Порука"),
            new("lt", "Taip", "Ne", "Gerai", "Atšaukti", "Išimtis", "Įvyko klaida", "Vieta", "Pranešimas"),
            new("lv", "Jā", "Nē", "Labi", "Atcelt", "Izņēmums", "Ir notikusi kļūda", "Atrašanās vieta", "Ziņojums"),
            new("et", "Jah", "Ei", "OK", "Tühista", "Erand", "Tekkis viga", "Asukoht", "Sõnum"),
            new("mk", "Да", "Не", "Во ред", "Откажи", "Исклучок", "Настана грешка", "Локација", "Порака"),
            new("sq", "Po", "Jo", "OK", "Anulo", "Përjashtim", "Ndodhi një gabim", "Vendndodhja", "Mesazh"),
            new("az", "Bəli", "Xeyr", "OK", "Ləğv et", "İstisna", "Xəta baş verdi", "Yer", "Mesaj"),
            new("bn", "হ্যাঁ", "না", "ঠিক আছে", "বাতিল করুন", "ব্যতিক্রম", "একটি ত্রুটি ঘটেছে", "অবস্থান", "বার্তা"),
            new("fa", "بله", "نه", "تایید", "لغو", "استثنا", "خطایی رخ داده است", "مکان", "پیام"),
            new("hi", "हाँ", "नहीं", "ठीक है", "रद्द करें", "अपवाद", "एक त्रुटि हुई", "स्थान", "संदेश"),
            new("ka", "დიახ", "არა", "კარგი", "გაუქმება", "გამონაკლისი", "მოხდა შეცდომა", "ადგილმდებარეობა", "შეტყობინება"),
            new("kk", "Иә", "Жоқ", "Жарайды", "Бас тарту", "Айырып алу", "Қате орын алды", "Орналасқан жері", "Хабарлама"),
            new("km", "បាទ/ចាស", "ទេ", "យល់ព្រម", "បោះបង់", "ករណីលើកលែង", "មានកំហុសបានកើតឡើង", "ទីតាំង", "សារ"),
            new("ky", "Ооба", "Жок", "Макул", "Баш тарт", "Айрыкча учур", "Ката кетти", "Жайгашкан жери", "Кабар"),
            new("lo", "ແມ່ນ", "ບໍ່", "ຕົກລົງ", "ຍົກເລີກ", "ຂໍ້ຍົກເວັ້ນ", "ເກີດຂໍ້ຜິດພາດ", "ສະຖານທີ່", "ຂໍ້ຄວາມ"),
            new("mn", "Тийм", "Үгүй", "OK", "Цуцлах", "Онцгой тохиолдол", "Алдаа гарлаа", "Байршил", "Зурвас"),
            new("my", "ဟုတ်ကဲ့", "မဟုတ်ပါ", "အိုကေ", "ပယ်ဖျက်", "ချွင်းချက်", "အမှားတစ်ခုဖြစ်ပွားခဲ့သည်", "တည်နေရာ",
                "သတင်းစကား"),
            new("ne", "हुन्छ", "हुँदैन", "ठीक छ", "रद्द गर्नुहोस्", "अपवाद", "त्रुटि भयो", "स्थान", "सन्देश"),
            new("pa", "ਹਾਂ", "ਨਹੀਂ", "ਠੀਕ ਹੈ", "ਰੱਦ ਕਰੋ", "ਅਪਵਾਦ", "ਇੱਕ ਗਲਤੀ ਆਈ ਹੈ", "ਸਥਾਨ", "ਸੁਨੇਹਾ"),
            new("si", "ඔව්", "නැත", "හරි", "අවලංගු කරන්න", "ව්යතිරේකය", "දෝෂයක් සිදුවී ඇත", "ස්ථානය", "පණිවිඩය"),
            new("sw", "Ndiyo", "Hapana", "Sawa", "Ghairi", "Tofauti", "Hitilafu imetokea", "Mahali", "Ujumbe"),
            new("ta", "ஆம்", "இல்லை", "சரி", "ரத்து செய்", "விலக்கு", "ஒரு பிழை ஏற்பட்டது", "இடம்", "செய்தி"),
            new("te", "అవును", "కాదు", "సరే", "రద్దు చేయి", "మినహాయింపు", "ఒక లోపం సంభవించింది", "స్థానం", "సందేశం"),
            new("ur", "جی ہاں", "نہیں", "ٹھیک ہے", "منسوخ کریں", "مستثنیٰ", "ایک خرابی پیش آئی", "مقام", "پیغام"),
            new("uz", "Ha", "Yo'q", "OK", "Bekor qilish", "Istisno", "Xatolik yuz berdi", "Manzil", "Xabar"),
            new("zh-TW", "是", "否", "確定", "取消", "例外", "發生了一個錯誤", "位置", "訊息")
        ];
    }
    
    