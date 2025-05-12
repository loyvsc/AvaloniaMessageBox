using System;
using Avalonia.Controls;
using CastelloBranco.AvaloniaMessageBox;

namespace AvaloniaMessageBoxTestApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        MessageBox.ShowAsync("Hello World on window opened !", "Hello World!", MessageBoxButtons.Ok, MessageBoxIcon.Success).GetAwaiter().GetResult();
    }
}