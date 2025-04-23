
# AvaloniaMessageBox

A flexible and modern modal message box implementation for Avalonia UI 11.2+.

Supports:
- Classic Desktop applications (`IClassicDesktopStyleApplicationLifetime`)
- Single view apps (`ISingleViewApplicationLifetime`)
- Early fallback before `MainWindow` or `MainView` is ready
- Native Fallback to OS API com P/Invoke para **macOS**, **Windows** e **X11/Linux** if avalonia is not avaliable
- DPI-aware sizing and theme-aware font and styling
- Optional icons and multi-line messages
- Modal behavior even in SingleView via panel overlay

## ✨ Features

- Compatible with **.NET 9** and **Avalonia 11.2+**
- Fully theme-aware (fonts, colors, shadows)
- Automatically localizes button text (`Yes`, `No`, `OK`, `Cancel`) using `CultureInfo.CurrentUICulture`
- Supports:
  - `OK`
  - `OK / Cancel`
  - `Yes / No`
- Built-in modern emoji-based icons:
  - ℹ️ Info
  - ⚠️ Warning
  - ❌ Error
  - ✅ Success
  - ❓ Question
  - 🛑✋ Stop (**hand over stop sign** with overlay)
- Works even **before** `MainWindow` or `MainView` is set

## 🌍 Localization Support

Button labels (`OK`, `Cancel`, `Yes`, `No`) are **automatically translated** based on the current UI culture (`CultureInfo.Current`).  
Includes built-in translations for over **68 languages**, including:

- pt-BR → "Sim", "Não", "OK", "Cancelar"
- es → "Sí", "No", "OK", "Cancelar"
- fr → "Oui", "Non", "OK", "Annuler"
- de → "Ja", "Nein", "OK", "Abbrechen"
- ja → "はい", "いいえ", "OK", "キャンセル"
- ...and many others

No configuration needed — just set the culture in your app startup code:

## 🚀 Uso

```csharp
await MessageBox.ShowAsync(
    parent: this, // Pode ser Window ou UserControl
    title: "Atenção",
    message: "Tem certeza que deseja continuar?",
    buttons: MessageBoxButtons.YesNo,
    icon: MessageBoxIcon.Question);
```

You can call it even before setting `MainWindow` in your `App.cs`:

Automatic Fallback to native os MessageBox if Avalonia is not ready

```csharp
await MessageBox.ShowAsync(null, "Erro de Inicialização", "Configuração inválida.");
```

## 🛑 Show Exception with details  

```csharp
try
{
    // ...
}
catch (Exception ex)
{
    await ExceptionMessageBox.ShowExceptionDialogAsync(this, ex);
}
```

Includes name of exception, line of error, filename and exception message.

## 📦 Install

### ✅ NuGet

```bash
dotnet add package CastelloBranco.AvaloniaMessageBox
```

Or in `.csproj`:

```xml
<PackageReference Include="CastelloBranco.AvaloniaMessageBox" Version="1.0.0" />
```

## 📄 License

[MIT License](LICENSE.txt)  
(c) 2025 **Castello Branco Tecnologia**

## 🏷️ Crédits

Created and mainteined by **Castello Branco Tecnologia**  

[GitHub - CastelloBrancoTecnologia/AvaloniaMessageBox](https://github.com/CastelloBrancoTecnologia/AvaloniaMessageBox)
