
from dataclasses import dataclass
from typing import Tuple, Optional
from models import Size

@dataclass
class FilmSettings:
    resolution: Size = Size(1920, 1080)           # width, height in pixels
    duration: float = 0.0                        # in seconds
    background_color: str = "black"               # e.g., "black", "#000000", "transparent"
    framerate: int = 30                           # default fps
    output_path: str = "output.mp4"               # default output file
    pixel_format: str = "yuv420p"                 # e.g., "yuv420p", "rgba"
    allow_transparency: bool = False              # toggle alpha channel (yuv420p doesn't support it)
    debug: bool = False                           # print ffmpeg command only
    use_gpu: bool = True                   # use GPU acceleration if available

    @property
    def video_codec(self) -> str:
        """
        Get the video codec based on the settings.
        """
        return "h264_nvenc" if self.use_gpu else "libx264"

    @property
    def audio_codec(self) -> str:
        """
        Get the audio codec based on the settings.
        """
        return "aac"

    @property
    def output(self) -> str:
        return self.output_path

    @output.setter
    def output(self, value: str):
        self.output_path = value

    def to_dict(self):
        return {
            "resolution": self.resolution,
            "duration": self.duration,
            "background_color": self.background_color,
            "framerate": self.framerate,
            "output_path": self.output_path,
            "pixel_format": self.pixel_format,
            "allow_transparency": self.allow_transparency,
            "debug": self.debug
        }

    @staticmethod
    def from_dict(data: dict) -> "FilmSettings":
        return FilmSettings(
            resolution=tuple(data.get("resolution", (1280, 720))),
            duration=data.get("duration", 10.0),
            background_color=data.get("background_color", "black"),
            framerate=data.get("framerate", 30),
            output_path=data.get("output_path", "output.mp4"),
            pixel_format=data.get("pixel_format", "yuv420p"),
            allow_transparency=data.get("allow_transparency", False),
            debug=data.get("debug", False)
        )
