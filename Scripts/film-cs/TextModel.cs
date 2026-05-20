namespace FilmCS
{
    public class TextModel
    {
        public string Text { get; set; }
        public int FontSize { get; set; } = 24;
        public string FontColor { get; set; } = "white";
        public string FontName { get; set; } = "MicrosoftYaHei";
        public double Alpha { get; set; } = 1.0;
        public bool ShowShadow { get; set; } = false;
        public string ShadowColor { get; set; } = "black";
        public int ShadowX { get; set; } = 2;
        public int ShadowY { get; set; } = 2;

        public TextModel(string text)
        {
            Text = text;
        }
    }
}
