from dataclasses import dataclass, field
from track import Track, FilterInfo
from commandBuilderContext import CommandBuilderContext, FilterResult
from models import Size, Location, TextModel, Align, Alignment, Margin, MediaFile
from logger import Logger
from fonts import Fonts
from typing import List
import subprocess

@dataclass
class OverlayTrack(Track):
    '''
    Base class for overlay tracks that can be used to overlay images or videos.
    This class is not meant to be instantiated directly, but serves as a base for specific overlay types.
    '''
    def __post_init__(self):
        super().__post_init__()
        self.is_overlay = True

class ComboTrack(Track):

    def get_inputs(self, context:CommandBuilderContext) -> List[str]:
        return []

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        return FilterResult('', '')

@dataclass
class ImageTrack(Track):
    path: str = field(default=None)
    position: Location = Location(0, 0)
    size: Size = None
    transition: str = None
    transition_duration: float = 0.0
    alpha: float = 1.0

    def get_inputs(self, context:CommandBuilderContext) -> List[str]:
        image_duration = self.duration + (self.transition_duration if self.transition else 0)
        context.settings.duration += image_duration

        return ['-loop', '1',  '-t', str(image_duration), '-i', self.path]

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        size = self.size if self.size else context.settings.resolution

        self.filter_info.input_index = context.input_index
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.output_label = f'[v{context.input_index}]'
        end_label = f'[v{context.input_index}]'

        filters = []

        if size:
            filters.append(f'scale={size.width}:{size.height}')

        if self.alpha < 1.0:
            filters.append(f'format=rgba,colorchannelmixer=aa={self.alpha}')
        else:
            filters.append(f'format={context.settings.pixel_format}')

        self.filter_info.filters = filters
        context.current_label = end_label

        context.connect_tracks.append(self)

        return FilterResult('', self.filter_info.output_label)

    def __repr__(self):
        return f'ImageTrack: {self.path}'

@dataclass
class ImageOverlayTrack(OverlayTrack):
    path: str = field(default=None)
    margin: Margin = Margin(0, 0, 0, 0)
    align: Align = Align.RIGHT
    size: Size = Size(64,64)
    alpha: float = 1.0

    def get_inputs(self, context: CommandBuilderContext) -> List[str]:
        return ['-loop', '1', '-i', self.path]

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        size = self.size if self.size else context.settings.resolution
        location = Alignment.get_location(self.align, context.settings.resolution, size, self.margin)

        result_label = f'[ov{context.input_index}]'

        self.filter_info.input_index = context.input_index
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.output_label = f'[ov{context.input_index}]'
        intermediate_label = f'[ov{context.input_index}_i]'

        filters = []
        if self.alpha < 1.0:
            filters.append(f'format=rgba,colorchannelmixer=aa={self.alpha}')
        if size:
            filters.append(f'scale={size.width}:{size.height}')

        context.current_label = result_label
        context.overlays_tracks.append(self)
        self.filter_info.filters = [
            f'{','.join(filters)}{intermediate_label}',
            f'{{context.current_label}}{intermediate_label}overlay=x={location.x}:y={location.y}'
        ]

        return FilterResult('', result_label)

    def __repr__(self):
        return f'ImageOverlay: {self.path} {self.filter_info}'

@dataclass
class TextTrack(Track):
    text: TextModel = TextModel('')
    location: Location = Location(0, 0)

    def __post_init__(self):
        super().__post_init__()
        self.has_input = False

    def get_inputs(self, context:CommandBuilderContext) -> list:
        return []

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        font_file = Fonts.resolve(self.text.fontname).escape_path()
        filters = []

        filters.append(f'drawtext=fontfile="{font_file}":')
        filters.append(f'text="{self.text.escape_text()}":')
        filters.append(f'fontsize={self.text.fontsize}:')
        filters.append(f'x={self.location.x}:')
        filters.append(f'y={self.location.y}:')

        if self.text.alpha < 1.0:
            filters.append(f'fontcolor={self.text.fontcolor}@{self.text.alpha}:')
        else:
            filters.append(f'fontcolor={self.text.fontcolor}:')

        if self.text.show_shadow:
            filters.append(f'shadowx={self.text.shadowx}:')
            filters.append(f'shadowy={self.text.shadowy}:')
            filters.append(f'shadowcolor={self.text.shadowcolor}:')

        self.filter_info.input_index = context.input_index
        self.filter_info.output_label = f'[t{context.input_index}]'
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.filters = filters

        context.overlays_tracks.append(self)

        return FilterResult('', self.filter_info.output_label)

    def __repr__(self):
        return f'Text: {self.text.text}'

@dataclass
class TextGroupTrack(Track):
    texts: list[TextModel] = field(default_factory=list)
    align: Align = Align.CENTER
    margin: Margin = Margin(0, 0, 0, 0)
    line_spacing: int = 10
    alpha: float = 1.0

    def __post_init__(self):
        super().__post_init__()
        self.has_input = False

    def get_inputs(self, context:CommandBuilderContext) -> list:
        return []

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        model_filters = []
        result_label = f'[tg{context.input_index}]'
        text_sizes: dict[int, Size] = self.calculate_text_size()
        max_width = max(size.width for size in text_sizes.values())

        margin = self.margin.copy()
        filters = []
        for i, text_model in enumerate(self.texts):
            font_file = Fonts.resolve(text_model.fontname).escape_path()
            size = text_sizes[i]
            location = Alignment.get_location(self.align, context.settings.resolution, size, margin, max_width)

            self.logger.info(f'TextGroupTrack: {text_model.text}, size: {size}, location: {location}')

            model_filters = []
            model_filters.append(f"drawtext=fontfile='{font_file}'")
            model_filters.append(f"text='{text_model.escape_text()}'")
            model_filters.append(f'fontsize={text_model.fontsize}')
            if text_model.alpha < 1.0:
                model_filters.append(f'fontcolor={text_model.fontcolor}@{text_model.alpha}')
            else:
                model_filters.append(f'fontcolor={text_model.fontcolor}')
            model_filters.append(f'x={location.x}')
            model_filters.append(f'y={location.y}')

            if text_model.show_shadow:
                model_filters.append(f'shadowcolor={text_model.shadowcolor}')
                model_filters.append(f'shadowx={text_model.shadowx}')
                model_filters.append(f'shadowy={text_model.shadowy}')

            model_filter = ':'.join(model_filters)

            margin.top += size.height + self.line_spacing

            filters.append(model_filter)

        filter = f'{','.join(filters)}'

        self.filter_info.input_index = context.input_index
        self.filter_info.output_label = result_label
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.filters = [filter]

        context.current_label = result_label
        context.overlays_tracks.append(self)

        self.logger.info(f'TextGroupTrack filter: {filter}, current label: {result_label}')
        return FilterResult('', result_label)

    def calculate_text_size(self) -> dict[int, Size]:
        text_sizes: dict[int, Size] = {}
        for i, text_model in enumerate(self.texts):
            size = text_model.get_text_size()
            text_sizes[i] = size
        return text_sizes

    def __repr__(self):
        return f'TextGroup: {", ".join(text.text for text in self.texts)}'

@dataclass
class AudioTrack(Track):
    path: str = field(default=None)

    def __post_init__(self):
        super().__post_init__()
        self.is_audio = True

    def get_inputs(self, context:CommandBuilderContext) -> list:
        duration = self.duration if self.duration else self.get_audio_duration(self.path)
        context.settings.duration += duration

        return ['-i', self.path]

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        offset = int(self.start * 1000)
        self.filter_info.input_index = context.input_index
        self.filter_info.output_label = f'[aud{context.input_index}]'
        self.filter_info.input_label = f'[{context.input_index}:a]'
        self.filter_info.filters = [f'adelay={offset}|{offset}']

        context.audio_tracks.append(self)
        if self.start > 0:
            return FilterResult(f'adelay={offset}|{offset}', self.filter_info.output_label)
        else:
            return FilterResult('', '')

    def get_audio_duration(self, path: str) -> float:
        '''Return the duration of an audio file in seconds.'''
        try:
            result = subprocess.run(
                [
                    'ffprobe',
                    '-v', 'error',
                    '-show_entries', 'format=duration',
                    '-of', 'default=noprint_wrappers=1:nokey=1',
                    path
                ],
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                check=True
            )
            return float(result.stdout.strip())
        except (subprocess.CalledProcessError, ValueError) as e:
            self.logger.error(f'Failed to get duration for {path}: {e}')
            raise ValueError(f'Invalid audio file or duration: {path}')

    def __repr__(self):
        return f'AudioTrack: {self.path} {self.filter_info}'

@dataclass
class VideoTrack(Track):
    path: str = field(default=None)
    location: Location = Location(0, 0)
    size: Size = None
    alpha: float = 1.0
    fade_in: float = 0.0
    fade_out: float = 0.0
    transition: str = None
    transition_duration: float = 0.0

    def get_inputs(self, context:CommandBuilderContext) -> list:
        if self.duration == 0:
            return ['-stream_loop', '-1', '-i', self.path]

        video_duration = self.get_video_duration(self.path)
        context.settings.duration += video_duration
        return ['-ss', str(self.start), '-t', str(self.duration), '-i', self.path]

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        size = self.size if self.size else context.settings.resolution
        duration = self.duration if self.duration else self.get_video_duration(self.path)

        filters = []
        if size:
            filters.append(f'scale={size.width}:{size.height}')

        if self.alpha < 1.0:
            filters.append(f'format=rgba,colorchannelmixer=aa={self.alpha}')
        else:
            filters.append(f'format={context.settings.pixel_format}')

        if self.fade_in:
            filters.append(f'fade=t=in:st=0:d={self.fade_in}')
        if self.fade_out:
            filters.append(f'fade=t=out:st={duration - self.fade_out}:d={self.fade_out}')

        result_label = f'[v{context.input_index}]'
        filter = f'{','.join(filters)}'

        self.filter_info.input_index = context.input_index
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.output_label = result_label
        self.filter_info.filters = [filter]
        context.connect_tracks.append(self)

        self.logger.info(f'Video filter: {filter}')
        return FilterResult('', result_label)

    def get_video_duration(self, path: str) -> float:
        '''Returns the duration of a video file in seconds as a float.'''
        result = subprocess.run(
            [
                'ffprobe',
                '-v', 'error',
                '-show_entries', 'format=duration',
                '-of', 'default=noprint_wrappers=1:nokey=1',
                path
            ],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        return float(result.stdout.strip())

@dataclass
class RepeatVideoTrack(Track):
    audio_path: str = field(default=None)
    video_path: str = field(default=None)
    size: Size = None
    alpha: float = 1.0
    fade_in: float = 0.0
    fade_out: float = 0.0
    transition: str = None

    def __post_init__(self):
        super().__post_init__()
        self.has_input = True
        if not self.audio_path:
            raise ValueError('Audio path must be provided for RepeatVideoTrack.')

        self.audio_file = MediaFile(self.audio_path)
        self.video_file = MediaFile(self.video_path)
        if not self.audio_file.path:
            raise ValueError('Invalid audio file path provided.')

    def get_inputs(self, context:CommandBuilderContext) -> List[str]:
        audio_duration = self.audio_file.get_duration()
        return ['-stream_loop', '-1', '-i', self.video_path]

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        size = self.size if self.size else context.settings.resolution
        duration = self.audio_file.get_duration() # self.duration if self.duration else self.video_file.get_duration()

        filters = []
        if size:
            filters.append(f'scale={size.width}:{size.height}')

        filters.append( f'trim=duration={duration}')

        if self.alpha < 1.0:
            filters.append(f'format=rgba,colorchannelmixer=aa={self.alpha}')
        else:
            filters.append(f'format={context.settings.pixel_format}')

        if self.fade_in:
            filters.append(f'fade=t=in:st=0:d={self.fade_in}')
        if self.fade_out:
            filters.append(f'fade=t=out:st={duration - self.fade_out}:d={self.fade_out}')

        result_label = f'[v{context.input_index}]'
        filter = f'{','.join(filters)}'

        self.filter_info.input_index = context.input_index
        self.filter_info.input_label = f'[{context.input_index}:v]'
        self.filter_info.output_label = result_label
        self.filter_info.filters = [filter]
        context.connect_tracks.append(self)

        self.logger.info(f'Video filter: {filter}')
        return FilterResult('', result_label)

@dataclass
class VideoOverlayTrack(OverlayTrack):
    path: str = field(default=None)
    location: Location = Location(0, 0)
    size: Size = None
    alpha: float = 1.0

    def get_inputs(self, context: CommandBuilderContext) -> List[str]:
        cmd = ['-stream_loop', '-1', '-i', self.path]
        if self.duration:
            cmd = ['-t', str(self.duration)] + cmd
        return cmd

    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        location = self.location
        size = self.size if self.size else context.settings.resolution

        filters = []

        result_label = f'[ov{context.input_index}]'

        self.filter_info.input_index = context.input_index
        self.filter_info.output_label = f'[ov{context.input_index}_v]'
        self.filter_info.input_label = f'[{context.input_index}:v]'
        intermediate_label = f'[ov{context.input_index}_i]'

        filters.append(f'format=rgba')

        if self.alpha < 1.0:
            filters.append(f'colorchannelmixer=aa={self.alpha}')

        filters.append(f'scale={size.width}:{size.height}')
        self.filter_info.filters = [
            f'{','.join(filters)}{intermediate_label}',
            f'{{context.current_label}}{intermediate_label}overlay={location.x}:{location.y}'
        ]
        context.current_label = result_label

        context.overlays_tracks.append(self)


        return FilterResult('', self.filter_info.output_label )

    def __repr__(self):
        return f'VideoOverlayTrack: {self.path} {self.filter_info}'
