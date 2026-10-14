using Avalonia.Controls;

using Framework.Themes;
using Framework.ViewModel;

namespace Framework.View
{
    public partial class MagnifierWindow : ChildWindow
    {
        private readonly MagnifierVM _magnifierVM;

        public MagnifierWindow()
        {
            InitializeComponent();

            _magnifierVM = new MagnifierVM();
            DataContext = _magnifierVM;
        }

        public void SetTheme(ITheme theme)
        {
            _magnifierVM.Theme = theme;
        }

        public void Update()
        {
            _magnifierVM.RefreshMagnifierImages();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _magnifierVM.Cleanup();
        }
    }
}
