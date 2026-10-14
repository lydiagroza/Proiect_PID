using Emgu.CV;
using System;
using System.Linq;
using Avalonia.Controls;

using Framework.ViewModel;

namespace Framework.View
{
    public partial class SliderWindow : ChildWindow
    {
        private readonly SliderVM _sliderVM;

        // For the XAML previewer.
        public SliderWindow()
        {
            InitializeComponent();
        }

        public SliderWindow(MainVM mainVM, string description)
            : this()
        {
            CloseOtherSliderWindows();

            _sliderVM = new SliderVM(mainVM);

            _sliderVM.Description = description;

            DataContext = _sliderVM;
        }

        public void SetWindowData<InColor, OutColor>(Image<InColor, byte> image,
            Func<Image<InColor, byte>, double, Image<OutColor, byte>> algorithm)
            where InColor : struct, IColor
            where OutColor : struct, IColor
        {
            _sliderVM.Image = image;
            _sliderVM.Algorithm = algorithm;
        }

        public void ConfigureSlider(double minimumValue = 0, double maximumValue = 255, double value = 0, double frequency = 5)
        {
            _sliderVM.MinimumValue = minimumValue;
            _sliderVM.MaximumValue = maximumValue;
            _sliderVM.Value = value;
            _sliderVM.Frequency = frequency;
        }

        private void CloseOtherSliderWindows()
        {
            foreach (var window in UiHelper.Windows.OfType<SliderWindow>().Where(w => w != this).ToList())
                window.Close();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _sliderVM?.Cleanup();
        }
    }
}
