namespace AudioBookStudio.Films.Models;
public class AudioTrack : Track
{
    private MediaFile? _mediaFile;
    public string? Path { get; set; }
    public bool AppendDuration { get; set; } = true;

    public AudioTrack()
    {
        IsAudio = true;
    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        _mediaFile = new MediaFile(Path!);

        if (AppendDuration)
        {
            var duration = _mediaFile.GetDuration();
            context.Settings.Duration += duration;
        }

        return ["-i", $"\"{Path}\""];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        int offset = (int)(Start * 1000);

        Filter.InputIndex = context.InputIndex;
        Filter.OutputLabel = $"[aud{context.InputIndex}]";
        Filter.InputLabel = $"[{context.InputIndex}:a]";
        Filter.Filters = [$"adelay={offset}|{offset}"];

        context?.AudioTracks?.Add(this);
    }

    public override string ToString()
    {
        return $"AudioTrack: {Path}";
    }
}