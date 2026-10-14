using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Framework.View;

namespace Framework
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                desktop.MainWindow = new MainWindow();
            }

            Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;

            base.OnFrameworkInitializationCompleted();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (UiHelper.MainWindow == null || !UiHelper.MainWindow.IsVisible)
                return;

            MessageBox.Show(e.Exception.ToString(), "Error");
            e.Handled = true;
        }
    }
}
