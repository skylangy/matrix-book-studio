using System.Collections.Generic;

namespace FilmCS
{
    public abstract class Track
    {
        public double Start { get; set; } = 0.0;
        public double Duration { get; set; } = 0.0;
        public Logger Logger { get; set; }
        public bool IsOverlay { get; set; } = false;
        public bool IsAudio { get; set; } = false;
        public bool HasInput { get; set; } = true;
        public FilterInfo FilterInfo { get; set; } = new FilterInfo();

        public Track()
        {
            Logger = new Logger(this.GetType().Name);
            FilterInfo = new FilterInfo();
        }

        public abstract List<string> GetInputs(CommandBuilderContext context);
        public abstract object GetFilter(CommandBuilderContext context);

        public override string ToString()
        {
            return $"{this.GetType().Name}(start={Start}, duration={Duration})";
        }
    }
}
