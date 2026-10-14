using Avalonia.Controls;
using Avalonia.Interactivity;

using Framework.ViewModel;

namespace Framework.View
{
    public partial class MessageBoxWindow : ChildWindow
    {
        private readonly MessageBoxVM _messageBoxVM;

        // For the XAML previewer.
        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        public MessageBoxWindow(string message, string caption)
            : this()
        {
            _messageBoxVM = new MessageBoxVM
            {
                Message = message,
                Caption = caption
            };
            if (UiHelper.MainWindow?.DataContext is MainVM mainVM)
                _messageBoxVM.Theme = mainVM.Theme;

            DataContext = _messageBoxVM;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _messageBoxVM?.Cleanup();
        }
    }
}
