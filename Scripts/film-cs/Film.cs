using System.Collections.Generic;

namespace FilmCS
{
    public class Film
    {
        public List<Track> Tracks { get; set; } = new List<Track>();
        public FilmSettings Settings { get; set; }

        public Film(FilmSettings settings = null)
        {
            Settings = settings ?? new FilmSettings();
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
        }

        public Film AddImage(string path, double start = 0.0, double duration = 4.5, string transition = null, double transitionDuration = 0.5, double alpha = 1.0)
        {
            // TODO: Implement transition validation if needed
            var imageTrack = new ImageTrack
            {
                Path = path,
                Start = start,
                Duration = duration,
                Transition = transition,
                TransitionDuration = transitionDuration,
                Alpha = alpha
            };
            AddTrack(imageTrack);
            return this;
        }

        public Film AddImagesFromFolder(string folder, double imageDuration = 5.0, string transition = null, double transitionDuration = 0.5, double alpha = 1.0)
        {
            // TODO: Implement folder reading and sorting
            return this;
        }

        public Film AddImageOverlay(string path, Margin margin = null, Size size = null, double alpha = 1.0, Align align = Align.Right)
        {
            var overlayImageTrack = new ImageOverlayTrack
            {
                Path = path,
                Size = size,
                Margin = margin ?? new Margin(),
                Alpha = alpha,
                Align = align
            };
            AddTrack(overlayImageTrack);
            return this;
        }

        public Film AddVideoOverlay(string path, double start = 0, double? duration = null, Location location = null, Size size = null, double alpha = 1.0, string transition = null, double transitionDuration = 0)
        {
            var overlayVideoTrack = new VideoOverlayTrack
            {
                Path = path,
                Start = start,
                Duration = duration ?? 0,
                Location = location ?? new Location(0, 0),
                Size = size,
                Alpha = alpha
            };
            AddTrack(overlayVideoTrack);
            return this;
        }

        public Film AddTextLayer(TextModel text, double start = 0, double duration = 0, Location location = null, string fontname = "MicrosoftYaHei", int fontsize = 24, string fontcolor = "white", double alpha = 1.0)
        {
            var textTrack = new TextTrack
            {
                Text = text,
                Start = start,
                Duration = duration,
                Location = location ?? new Location(0, 0)
            };
            AddTrack(textTrack);
            return this;
        }

        public Film AddTextGroup(List<TextModel> texts, double start = 0, double duration = 0, Margin margin = null, int lineSpacing = 10, double alpha = 1.0, Align align = Align.Center)
        {
            var textGroupTrack = new TextGroupTrack
            {
                Texts = texts,
                Start = start,
                Duration = duration,
                Margin = margin ?? new Margin(),
                LineSpacing = lineSpacing,
                Align = align,
                Alpha = alpha
            };
            AddTrack(textGroupTrack);
            return this;
        }

        public Film AddAudioTrack(string path, double start = 0.0, double duration = 0.0)
        {
            var audioTrack = new AudioTrack
            {
                Path = path,
                Start = start,
                Duration = duration
            };
            AddTrack(audioTrack);
            return this;
        }

        public Film AddRepeatVideoTrack(string videoPath, string audioPath, double start = 0.0, double duration = 0.0)
        {
            var repeatVideoTrack = new RepeatVideoTrack
            {
                AudioPath = audioPath,
                VideoPath = videoPath,
                Start = start,
                Duration = duration
            };
            AddTrack(repeatVideoTrack);
            return this;
        }

        public Film AddVideoTrack(string path, double start = 0.0, double duration = 0.0, double fadeIn = 0.0, double fadeOut = 0.0, string transition = null, double transitionDuration = 0.0)
        {
            var videoTrack = new VideoTrack
            {
                Path = path,
                Start = start,
                Duration = duration,
                FadeIn = fadeIn,
                FadeOut = fadeOut,
                Transition = transition,
                TransitionDuration = transitionDuration
            };
            AddTrack(videoTrack);
            return this;
        }

        public void SetDebug(bool value = true)
        {
            Settings.Debug = value;
        }
    }
}
