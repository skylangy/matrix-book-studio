using AudioBookStudio.Films.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AudioBookStudio.Films.Models;

public class CommandBuilder
{
    private readonly Film _film;
    private readonly List<Track> _tracks;
    private readonly ILogger? _logger;
    private readonly string _ffmpegBin;

    public CommandBuilder(Film film, ILogger? logger = null)
    {
        _film = film;
        _tracks = film.Tracks;
        _logger = logger;
        _ffmpegBin = FindFfmpeg();

        _logger ??= LoggerFactory.Create(builder => { builder.AddConsole().AddDebug(); })
                                 .CreateLogger<CommandBuilder>();
    }

    public FilterList Build(CommandBuilderContext context)
    {
        var cmd = new FilterList { _ffmpegBin, "-loglevel", "error" };
        if (context.Overwrite)
            cmd.Add("-y");
        var filterComplex = new FilterList();
        foreach (var track in _tracks)
        {
            var inputs = track.BuildInputs(context);
            cmd.AddRange(inputs);

            track.BuildFilter(context);
            if (track.HasInput)
                context.InputIndex++;
        }
        BuildInputs(cmd, context);
        BuildConnects(filterComplex, context);
        BuildOverlays(filterComplex, context);
        BuildAudio(filterComplex, context);
        filterComplex = filterComplex.Clean();
        PrintValues("Filter complex:", filterComplex);
        if (filterComplex.HasItems())
        {
            cmd.Add("-filter_complex");
            cmd.Add(string.Join(";", filterComplex));
        }

        if (!string.IsNullOrEmpty(context.CurrentLabel))
        {
            cmd.Add("-map");
            cmd.Add(context.CurrentLabel);
        }

        if (!string.IsNullOrEmpty(context.CurrentAudioLabel))
        {
            cmd.Add("-map");
            cmd.Add(context.CurrentAudioLabel);
        }
        cmd.AddRange(
        [
            "-metadata", $"title={_film.Title ?? string.Empty}".QuotePath(),
            "-metadata", $"artist={_film.Artist ?? string.Empty}".QuotePath(),
            "-c:v", context.Settings.VideoCodec,
            "-pix_fmt", context.Settings.PixelFormat,
            "-r", context.Settings.Framerate.ToString(),
            "-c:a", context.Settings.AudioCodec,
            "-t", context.Settings.Duration.ToString(),
            context.Output.QuotePath()
        ]);

        _logger?.LogInformation("Final command: \n{Command}\n", string.Join(" ", cmd));
        return cmd;
    }

    private string FindFfmpeg()
    {
        try
        {
            var process = Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            _logger?.LogInformation("FFmpeg found: {Line}", process?.StandardOutput.ReadLine());
            return "ffmpeg";
        }
        catch
        {
            _logger?.LogError("FFmpeg not found. Please install FFmpeg and ensure it's in your PATH.");
            throw new Exception("FFmpeg is required but not found.");
        }
    }

    private void BuildInputs(List<string> cmd, CommandBuilderContext context)
    {
        _logger?.LogInformation("Building inputs: {Label}", context.CurrentLabel);
        cmd.AddRange(context.Inputs);
        PrintValues("Inputs:", context.Inputs);
        _logger?.LogInformation("");
    }

    private void BuildConnects(List<string> filterComplex, CommandBuilderContext context)
    {
        _logger?.LogInformation("Building connects: {Label}", context.CurrentLabel);

        Track? preTrack = null;
        string? endLabel = null;
        var transitions = new List<string>();
        var concats = new List<string>();

        foreach (var track in context.ConnectTracks)
        {
            var trackFilter = $"{track.Filter.InputLabel}{string.Join(",", track.Filter.Filters)}{track.Filter.OutputLabel}";
            _logger?.LogInformation("Track filter: {Filter}", trackFilter);

            filterComplex.Add(trackFilter);

            if (preTrack != null)
            {
                var currentLabel = endLabel ?? preTrack.Filter.OutputLabel ?? string.Empty;
                endLabel = $"[xf{track.Filter.InputIndex}]";

                if (track.Start == 0)
                    track.Start = preTrack.Start + preTrack.Duration;

                if (preTrack.HasTransition)
                {
                    double offset = preTrack.Duration - preTrack.TransitionDuration;
                    _logger?.LogInformation("Transition Offset: {start} + {duration} - {transitionDuration} = {Offset}", preTrack.Start, preTrack.Duration, preTrack.TransitionDuration, offset);
                    var transitionFilter = $"{currentLabel}{track.Filter.OutputLabel}xfade=transition={preTrack.Transition}:duration={preTrack.TransitionDuration}:offset={offset:0.##}{endLabel}";
                    transitions.Add(transitionFilter);
                }
                else
                {
                    concats.Add(track.Filter.OutputLabel);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(track.Transition))
                    concats.Add(track.Filter.OutputLabel);
            }
            preTrack = track;
        }

        PrintValues("Transitions:", transitions);
        PrintValues("Concats:", concats);

        filterComplex.AddRange(transitions);
        if (concats.HasItems())
        {
            var concatFilter = $"{string.Join("", concats)}concat=n={concats.Count}:v=1:a=0{endLabel}";
            filterComplex.Add(concatFilter);
            _logger?.LogInformation("Concat filter: {Filter}", concatFilter);
        }
        context.CurrentLabel = endLabel;
        PrintValues($"Filter complex after connects: {endLabel}", filterComplex);
        _logger?.LogInformation("");
    }

    private void BuildOverlays(List<string> filterComplex, CommandBuilderContext context)
    {
        _logger?.LogInformation("Building overlays: {Label}", context.CurrentLabel);
        var endLabel = context.CurrentLabel;
        var filters = new List<string>();
        foreach (var track in context.OverlaysTracks)
        {
            var startLabel = track.HasInput ? track.Filter.InputLabel : endLabel;
            _logger?.LogInformation("Start label: {start}, End label: {end}, Current label: {current}", startLabel, endLabel, context.CurrentLabel);
            var trackFilter = $"{startLabel}{string.Join(";", track.Filter.Filters)}{track.Filter.OutputLabel}".Replace("{ctx.CurrentLabel}", context.CurrentLabel);
            filters.Add(trackFilter);
            endLabel = track.Filter.OutputLabel;
            context.CurrentLabel = endLabel;
        }
        filterComplex.AddRange(filters);
        context.CurrentLabel = endLabel;
        PrintValues("Overlay filters:", filters);
        _logger?.LogInformation("");
    }

    private void BuildAudio(List<string> filterComplex, CommandBuilderContext context)
    {
        _logger?.LogInformation("Building audio tracks: {Label}", context.CurrentLabel);
        var filters = new List<string>();
        string currentLabel = string.Empty;
        foreach (var track in context.AudioTracks)
        {
            _logger?.LogInformation("Processing audio track: {track}", track);
            var trackFilter = $"{track.Filter.InputLabel}{string.Join(",", track.Filter.Filters)}{track.Filter.OutputLabel}";
            filters.Add(trackFilter);
            currentLabel = track.Filter.OutputLabel;
            _logger?.LogInformation("Audio filter: {filter}", trackFilter);
        }

        if (context.AudioTracks.Count > 1)
        {
            var outLabel = "[aout]";

            var concatLable = string.Join("", context.AudioTracks.Select(x => x.Filter.OutputLabel));

            filters.Add($"{concatLable}concat=n={context.AudioTracks.Count}:v=0:a=1{outLabel}");
            currentLabel = outLabel;
        }

        if (filters.HasItems())
            filterComplex.Add(string.Join(",", filters));
        context.CurrentAudioLabel = currentLabel;

        PrintValues("Audio end label:", [currentLabel]);
    }

    private void PrintValues(string message, IEnumerable<string> values)
    {
        _logger?.LogInformation("{message}", message);
        foreach (var value in values)
            _logger?.LogInformation("\t - {value}", value);
        _logger?.LogInformation("");
    }
}
