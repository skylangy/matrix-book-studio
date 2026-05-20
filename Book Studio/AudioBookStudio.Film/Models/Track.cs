namespace AudioBookStudio.Films.Models;

public abstract class Track
{
    public Track()
    {
        Filter = new FilterInfo();
    }

    public double Start { get; set; } = 0.0;
    public double Duration { get; set; } = 0.0;

    public double Alpha { get; set; } = 1.0;
    public string? Transition { get; set; }
    public double TransitionDuration { get; set; } = 0.0;
    public bool IsOverlay { get; set; } = false;
    public bool IsAudio { get; set; } = false;
    public virtual bool HasInput { get; set; } = true;
    public virtual FilterInfo Filter { get; set; } = new FilterInfo();

    public bool HasTransition
    {
        get
        {
            return !string.IsNullOrEmpty(Transition) && TransitionDuration > 0.0;
        }
    }

    public abstract FilterList BuildInputs(CommandBuilderContext context);

    public abstract void BuildFilter(CommandBuilderContext context);

    public override string ToString()
    {
        return $"{this.GetType().Name}(start={Start}, duration={Duration})";
    }
}
