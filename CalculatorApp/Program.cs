using Avalonia;
using Avalonia.Threading; // ✅ добавьте это пространство имён
using System;
using System.Diagnostics;

namespace CalculatorApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Глобальные необработанные в любом потоке
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[UnhandledException] {e.ExceptionObject}");
        };

        // Avalonia UI‑поток исключения
        Dispatcher.UIThread.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[UIThreadUnhandled] {e.Exception}");
            e.Handled = true;
        };
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
