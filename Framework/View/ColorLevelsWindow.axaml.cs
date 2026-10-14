using Avalonia.Controls;

using Framework.ViewModel;
using Framework.ViewModel.ColorLevels;

namespace Framework.View
{
    public partial class ColorLevelsWindow : ChildWindow
    {
        private readonly ColorLevelsVM _colorLevelsVM;

        // For the XAML previewer.
        public ColorLevelsWindow()
        {
            InitializeComponent();
        }

        public ColorLevelsWindow(MainVM mainVM, ColorLevelsType type)
            : this()
        {
            _colorLevelsVM = ColorLevelsFactory.Produce(type);
            _colorLevelsVM.Theme = mainVM.Theme;
            DataContext = _colorLevelsVM;

            Update();
        }

        public void Update()
        {
            _colorLevelsVM.UpdateSelectedPoint();
        }

        public void RefreshImages()
        {
            _colorLevelsVM.RefreshPlots();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _colorLevelsVM?.Cleanup();
        }
    }
}
