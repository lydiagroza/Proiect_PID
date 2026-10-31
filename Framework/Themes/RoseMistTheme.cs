namespace Framework.Themes
{
    internal class RoseMistTheme : ITheme
    {
        private RoseMistTheme() { }

        public static RoseMistTheme Instance { get; } = new RoseMistTheme();

        public string MenuBackgroundColor => "#EADBD5";

        public string MenuItemBackgroundColor => "#F4EAE6";

        public string MenuItemForegroundColor => "#4A3F45";

        public string BackgroundColor => "#FAF6F2";

        public string CanvasBackgroundColor => "#F1ECE8";

        public string CanvasBorderBrush => "#D9C7D0";

        public string TextForeground => "#5B4E55";

        public string ButtonBackgroundColor => "#E6D5DF";

        public string ButtonForegroundColor => "#4A3F45";

        public string ButtonBorderBrush => "#C9B3C1";
    }
}
