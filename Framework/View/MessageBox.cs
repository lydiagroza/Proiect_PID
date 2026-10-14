using System.Threading.Tasks;

namespace Framework.View
{
    /// <summary>
    /// Shows a message to the user in a small dialog, like WPF's MessageBox.
    /// </summary>
    public static class MessageBox
    {
        /// <summary>Shows the message and returns immediately (the dialog stays open until the user closes it).</summary>
        public static void Show(string messageBoxText)
        {
            _ = ShowAsync(messageBoxText);
        }

        /// <summary>Shows the message with a title and returns immediately.</summary>
        public static void Show(string messageBoxText, string caption)
        {
            _ = ShowAsync(messageBoxText, caption);
        }

        /// <summary>Shows the message; use <c>await MessageBox.ShowAsync(...)</c> to continue after the user closes it.</summary>
        public static Task ShowAsync(string messageBoxText, string caption = "Image Processing")
        {
            var window = new MessageBoxWindow(messageBoxText, caption);
            return window.ShowDialog();
        }
    }
}
