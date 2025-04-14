
# AvaloniaMessageBox

A flexible and modern modal message box implementation for Avalonia UI 11.2+.

Supports:
- Classic Desktop applications (`IClassicDesktopStyleApplicationLifetime`)
- Single view apps (`ISingleViewApplicationLifetime`)
- Early fallback before `MainWindow` or `MainView` is ready
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

Button labels (`OK`, `Cancel`, `Yes`, `No`) are **automatically translated** based on the current UI culture (`CultureInfo.CurrentUICulture`).  
Includes built-in translations for over **100 languages**, including:

- pt-BR → "Sim", "Não", "OK", "Cancelar"
- es → "Sí", "No", "OK", "Cancelar"
- fr → "Oui", "Non", "OK", "Annuler"
- de → "Ja", "Nein", "OK", "Abbrechen"
- ja → "はい", "いいえ", "OK", "キャンセル"
- ...and many others

No configuration needed — just set the culture in your app startup code:

```csharp
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
```

## 🚀 Usage

```csharp
await AvaloniaWindowedMessageBox.ShowAsync(
    parent: this, // Can be a Window or UserControl
    title: "Attention",
    message: "Are you sure you want to continue?",
    buttons: AvaloniaWindowedMessageBox.MessageBoxButtons.YesNo,
    icon: AvaloniaWindowedMessageBox.MessageBoxIcon.Question);
```

You can call it even before setting `MainWindow` in your `App.cs`:

```csharp
await AvaloniaWindowedMessageBox.ShowAsync(null, "Startup Error", "The configuration is invalid.");
```

## 🧪 Example with Stop Icon

```csharp
await AvaloniaWindowedMessageBox.ShowAsync(
    this,
    "Restricted",
    "Access denied.",
    AvaloniaWindowedMessageBox.MessageBoxButtons.Ok,
    AvaloniaWindowedMessageBox.MessageBoxIcon.Stop);
```

This renders a composite emoji with 🛑 background and ✋ overlaid at center.

## 🧩 Installation

Simply copy `AvaloniaWindowedMessageBox.cs` into your project.  
No dependencies or NuGet packages required.

## 📄 License

MIT License  
(c) 2024 Castello Branco Tecnologia

## 🏷️ Credits

Created and maintained by **Castello Branco Tecnologia**
