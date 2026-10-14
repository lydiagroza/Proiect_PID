using Avalonia.Controls;

using Framework.Model;
using Framework.ViewModel;

namespace Framework.View
{
    public partial class GraphWindow : ChildWindow
    {
        private readonly GraphVM _graphVM;

        // For the XAML previewer.
        public GraphWindow()
        {
            InitializeComponent();
        }

        public GraphWindow(MainVM mainVM, PointCollection points)
            : this()
        {
            _graphVM = new GraphVM();

            _graphVM.Theme = mainVM.Theme;
            _graphVM.CreateGraph(points.Clone());

            DataContext = _graphVM;
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _graphVM?.Cleanup();
        }
    }
}
