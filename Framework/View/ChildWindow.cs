using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Framework.View
{
    /// <summary>
    /// Base class of the secondary windows. They are owned by the main window,
    /// so they stay in front of it (like the windows of the WPF framework).
    /// </summary>
    public class ChildWindow : Window
    {
        // Use the look of a normal window.
        protected override Type StyleKeyOverride => typeof(Window);

        /// <summary>Shows the window in front of the main window.</summary>
        public new void Show()
        {
            Window owner = UiHelper.MainWindow;
            if (owner != null && owner != this && owner.IsVisible)
                Show(owner);
            else
                base.Show();
        }

        /// <summary>
        /// Shows the window as a dialog: the window that is in use stays disabled until the dialog is closed.
        /// Use <c>await window.ShowDialog();</c> to continue after the dialog is closed.
        /// </summary>
        public Task ShowDialog()
        {
            Window owner = UiHelper.ActiveWindow;
            if (owner != null && owner != this && owner.IsVisible)
                return ShowDialog(owner);

            var closed = new TaskCompletionSource<bool>();
            Closed += (s, e) => closed.TrySetResult(true);
            base.Show();
            return closed.Task;
        }
    }
}
