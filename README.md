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

- Compatible with .NET 9 and Avalonia 11.2+
- Uses theme fonts and colors
- Supports `OK`, `OK/Cancel`, and `Yes/No` button sets
- Icons: Info, Warning, Error, Success, Question, Stop
- Automatically adapts to application lifetime and environment

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

## 🧩 Installation

Simply copy `AvaloniaWindowedMessageBox.cs` into your project.

## 📄 License

MIT License

Copyright (c) 2024 Castello Branco Tecnologia

Permission is hereby granted, free of charge, to any person obtaining a copy  
of this software and associated documentation files (the “Software”), to deal  
in the Software without restriction, including without limitation the rights  
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell  
copies of the Software, and to permit persons to whom the Software is  
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all  
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR  
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE  
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER  
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE  
SOFTWARE.

## 🏷️ Credits

Created and maintained by **Castello Branco Tecnologia**
