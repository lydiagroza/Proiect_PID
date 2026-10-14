using System.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Interactivity;

using Framework.ViewModel;

namespace Framework.View
{
    /// <summary>
    /// Asks the user for numeric parameters. Usage:
    /// <code>
    /// var window = new DialogWindow(_mainVM, new List&lt;string&gt; { "Threshold:" });
    /// await window.ShowDialog();
    /// List&lt;double&gt; values = window.GetValues();   // null if the user cancelled
    /// </code>
    /// </summary>
    public partial class DialogWindow : ChildWindow
    {
        private readonly DialogVM _dialogVM;
        private bool _isFirstBoxFocused;
        private bool _isAccepted;

        // For the XAML previewer.
        public DialogWindow()
        {
            InitializeComponent();
        }

        public DialogWindow(MainVM mainVM, List<string> labels)
            : this()
        {
            _dialogVM = new DialogVM();

            _dialogVM.Theme = mainVM.Theme;
            _dialogVM.CreateParameters(labels);

            DataContext = _dialogVM;
        }

        public List<double> GetValues()
        {
            return _isAccepted ? _dialogVM.GetValues() : null;
        }

        #region Event handlers
        private void ParameterBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstBoxFocused)
                return;

            _isFirstBoxFocused = true;
            ((TextBox)sender).Focus();
        }

        private async void DoneClick(object sender, RoutedEventArgs e)
        {
            int invalidIndex = _dialogVM.FindInvalidParameter();
            if (invalidIndex >= 0)
            {
                await MessageBox.ShowAsync($"Please enter a number for \"{_dialogVM.Parameters[invalidIndex].ParamText.Trim()}\".");

                var parameterBox = FindParameterBox(invalidIndex);
                parameterBox?.Focus();
                parameterBox?.SelectAll();
                return;
            }

            _isAccepted = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _dialogVM?.Cleanup();
        }
        #endregion

        #region Helpers
        private TextBox FindParameterBox(int index)
        {
            var container = parameterList.ContainerFromIndex(index);
            return container?.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        }
        #endregion
    }
}
