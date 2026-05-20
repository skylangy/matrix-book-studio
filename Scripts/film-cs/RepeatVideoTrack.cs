using System.Collections.Generic;

namespace FilmCS
{
    public class RepeatVideoTrack : Track
    {
        public string AudioPath { get; set; }
        public string VideoPath { get; set; }
        public Size Size { get; set; }
        public double Alpha { get; set; } = 1.0;
        public double FadeIn { get; set; } = 0.0;
        public double FadeOut { get; set; } = 0.0;
        public string Transition { get; set; }

        public RepeatVideoTrack()
        {
            HasInput = true;
        }

        public override List<string> GetInputs(object context)
        {
            // Placeholder: actual implementation would depend on context types
            return new List<string>();
        }

        public override object GetFilter(object context)
        {
            // Placeholder: actual implementation would depend on context types
            return null;
        }
    }
}
