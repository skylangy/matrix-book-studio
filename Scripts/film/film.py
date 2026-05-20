import subprocess
import json
from typing import List, Optional
from pathlib import Path
from natsort import natsorted

from track import Track
from tracks import (ImageTrack, TextTrack, AudioTrack, VideoTrack,
                    VideoOverlayTrack, TextGroupTrack, ImageOverlayTrack,
                    RepeatVideoTrack)
from filmSettings import FilmSettings
from models import Location, Transitions, TextModel, Margin
from commandBuilderContext import CommandBuilderContext
from commandBuilder import CommandBuilder
from logger import Logger

class Film:
    def __init__(self, settings: Optional[FilmSettings] = None):
        self.tracks: List[Track] = []
        self.settings = settings or FilmSettings()
        self.logger = Logger(__name__)
        self.ffmpeg_bin = self._find_ffmpeg()
        self.is_gpu_supported = self._check_gpu_support()

    def add_track(self, track: Track):
        self.tracks.append(track)

    def add_image(self, path: str,
                  start: float = 0.0,
                  duration: float = 4.5,
                  transition: str = None,
                  transition_duration: float = 0.5,
                  alpha: float = 1.0) -> 'Film':
        '''
        Adds an ImageTrack to this Film instance.
        '''
        if transition:
            Transitions.validate_or_raise(transition)

        image_track = ImageTrack(
            path=path,
            start=start,
            duration=duration,
            transition=transition,
            transition_duration=transition_duration,
            alpha=alpha
        )
        self.add_track(image_track)
        return self

    def add_images_from_folder(self,
        folder: str,
        image_duration: float = 5.0,
        transition: str = None,
        transition_duration: float = 0.5,
        alpha: float = 1.0
    ) -> 'Film':
        '''
        Adds ImageTracks from a folder to this Film instance.
        '''
        files = [
            f for f in Path(folder).iterdir() if f.suffix.lower() in ['.jpg', '.jpeg']
        ]
        images = self.sort_files(files)

        if transition:
            Transitions.validate_or_raise(transition)

        current_time = 0.0

        for image in images:
            self.add_track(ImageTrack(
                path=str(image),
                start=current_time,
                duration=image_duration,
                transition=transition,
                transition_duration=transition_duration,
                alpha=alpha,
            ))
            current_time += image_duration - (transition_duration if transition else 0)

        return self

    def sort_files(self, files: List[str]) -> List[str]:
        return natsorted(files, key=lambda x: Path(x).name)

    def add_image_overlay(self,
                          path: str,
                          margin: Margin = Margin(0, 0, 0, 0),
                          size=None,
                          alpha=1.0,
                          align='right') -> 'Film':
        '''
        Adds an image overlay track.
        '''
        overlay_image_track = ImageOverlayTrack(
            path=path,
            size=size,
            margin=margin,
            alpha=alpha,
            align=align,
        )
        self.add_track(overlay_image_track)
        return self

    def add_video_overlay(self,
                          path: str,
                          start=0,
                          duration=None,
                          location=Location(0, 0),
                          size=None,
                          alpha=1.0,
                          transition=None,
                          transition_duration=0) -> 'Film':
        '''
        Adds a video overlay track.

        Args:
            path (str): Path to video file.
            start (float): Start time in seconds.
            duration (float): Duration in seconds. None means full video duration.
            x, y (int): Overlay position on output video.
            width, height (int): Resize overlay video. None means original size.
            alpha (float): Opacity from 0 (transparent) to 1 (opaque).
            transition (str): Transition effect between overlays (optional).
            transition_duration (float): Duration of transition effect.
        '''
        overlay_video_track = VideoOverlayTrack(
            path=path,
            start=start,
            duration=duration,
            location=location,
            size=size,
            alpha=alpha,

        )
        self.add_track(overlay_video_track)
        return self

    def add_text_layer(self, text, start=0, duration=0, location=Location(0, 0),
                       fontname='MicrosoftYaHei', fontsize=24, fontcolor='white', alpha=1.0) -> 'Film':
        '''
        Adds a text overlay track.

        Args:
            text (str): Text content to display.
            start (float): Start time in seconds.
            duration (float): Duration in seconds.
            x (int): X position of the text.
            y (int): Y position of the text.
            fontfile (str): Path to font file.
            fontsize (int): Font size.
            fontcolor (str): Font color (e.g. 'white', 'red', '#00ff00').
            alpha (float): Opacity (0.0 to 1.0).
        '''
        text_track = TextTrack(
            text=text,
            start=start,
            duration=duration,
            location=location,
            fontname=fontname,
            fontsize=fontsize,
            fontcolor=fontcolor,
            alpha=alpha,

        )
        self.add_track(text_track)
        return self

    def add_text_group(self, texts: List[TextModel], start=0, duration=0,
                       margin: Margin = Margin(0, 0, 0, 0),
                       line_spacing: int = 10,
                       alpha=1.0,
                       align = 'center') -> 'Film':
        '''
        Adds a group of text overlay tracks.

        Args:
            texts (List[TextModel]): List of TextModel instances.
            start (float): Start time in seconds.
            duration (float): Duration in seconds.
            location (Location): Position of the text.
            alpha (float): Opacity from 0 (transparent) to 1 (opaque).
        '''
        text_group_track = TextGroupTrack(
            texts=texts,
            start=start,
            duration=duration,
            margin=margin,
            line_spacing=line_spacing,
            align=align,
            alpha=alpha,

        )
        self.add_track(text_group_track)
        return self

    def add_audio_track(self, path: str, start: float = 0.0, duration: float = 0.0) -> 'Film':
        '''
        Adds an audio track.
        '''
        audio_track = AudioTrack(
            path=path,
            start=start,
            duration=duration,

        )
        self.add_track(audio_track)
        return self

    def add_repeat_video_track(self, video_path: str, audio_path: str, start: float = 0.0, duration: float = 0.0) -> 'Film':
        '''
        Adds a video track that repeats indefinitely.
        '''
        repeat_video_track = RepeatVideoTrack(
            audio_path=audio_path,
            video_path=video_path,
            start=start,
            duration=duration,
        )
        self.add_track(repeat_video_track)
        return self

    def add_video_track(self, path: str, start: float = 0.0, duration: float = 0.0,
                        fade_in: float = 0.0,
                        fade_out: float = 0.0,
                        transition: str = None,
                        transition_duration: float = 0.0) -> 'Film':
        '''
        Adds a video track.
        '''
        video_track = VideoTrack(
            path=path,
            start=start,
            duration=duration,
            fade_in=fade_in,
            fade_out=fade_out,
            transition=transition,
            transition_duration=transition_duration
        )
        self.add_track(video_track)
        return self

    def set_debug(self, value: bool = True):
        self.settings.debug = value

    def export_ffmpeg_command(self) -> List[str]:
        context = CommandBuilderContext(settings=self.settings)
        builder = CommandBuilder(self.tracks)
        cmd = builder.build(context)
        return cmd

    def run(self):
        cmd = self.export_ffmpeg_command()
        self.logger.info('Running FFmpeg command:\n\n%s\n%s \n', "=" * 100, ' '.join(cmd))

        if self.settings.debug:
            print('FFmpeg Command:\n', ' '.join(cmd))
        else:
            try:
                process = subprocess.run(cmd,
                            capture_output=True,
                                text=True,
                                check=True,
                                encoding='utf-8')

                if process.stdout:
                    self.logger.debug('FFmpeg stdout: %s', process.stdout)
            except subprocess.CalledProcessError as e:
                self.logger.info('=' * 100)
                self.logger.error("FFmpeg failed with return code %s", e.returncode)
                stderr_output = e.stderr  # Get stderr from the exception
                # Also log stdout if needed
                if e.stdout:
                    self.logger.debug("FFmpeg stdout: %s", e.stdout)

                for line in stderr_output.splitlines():
                    line = line.strip()
                    if not line:
                        continue

                    if '[fatal]' in line or '[panic]' in line:
                        self.logger.critical('\t FFmpeg: %s', line)
                    elif '[error]' in line:
                        self.logger.error('\t FFmpeg: %s', line)
                    elif '[warning]' in line:
                        self.logger.warning('\t FFmpeg: %s', line)
                    elif '[info]' in line:
                        self.logger.info('\t FFmpeg: %s', line)
                    elif '[verbose]' in line or '[debug]' in line or '[trace]' in line:
                        self.logger.debug('\t FFmpeg: %s', line)
                    else:
                        self.logger.info('\t FFmpeg: %s', line)

    def save_to_json(self, path: str):
        def to_dict(obj):
            d = obj.__dict__.copy()
            d['type'] = obj.__class__.__name__
            return d

        data = {
            'size': [self.width, self.height],
            'duration': self.duration,
            'bg_color': self.bg_color,
            'tracks': [to_dict(t) for t in self.tracks]
        }
        Path(path).write_text(json.dumps(data, indent=2, ensure_ascii=False))

    @staticmethod
    def load_from_json(path: str) -> 'Film':
        from_types = {
            'ImageTrack': ImageTrack,
            'TextTrack': TextTrack,
            'AudioTrack': AudioTrack,
            'VideoTrack': VideoTrack,
        }
        data = json.loads(Path(path).read_text(encoding='utf-8'))
        film = Film(tuple(data['size']), data['duration'], data.get('bg_color', 'black'))
        for track_data in data['tracks']:
            cls = from_types[track_data.pop('type')]
            film.add_track(cls(**track_data))
        return film

    def _find_ffmpeg(self) -> str:
        try:
            result = subprocess.run(['ffmpeg', '-version'], capture_output=True, text=True, check=True)
            self.logger.info('FFmpeg found: %s', result.stdout.split('\n')[0])
            return 'ffmpeg'
        except (subprocess.CalledProcessError, FileNotFoundError):
            self.logger.error('FFmpeg not found. Please install FFmpeg and ensure it\'s in your PATH.')
            raise FileNotFoundError('FFmpeg is required but not found.')

    def _check_gpu_support(self) -> bool:
        '''Check if NVIDIA GPU acceleration (NVENC) is supported.'''
        try:
            result = subprocess.run([self.ffmpeg_bin, '-encoders'], capture_output=True, text=True, check=True)
            if 'h264_nvenc' in result.stdout:
                self.logger.info('NVIDIA NVENC (h264_nvenc) is supported.')
                return True
            else:
                self.logger.warning('NVIDIA NVENC not supported. Falling back to CPU encoding.')
                return False
        except subprocess.CalledProcessError:
            self.logger.warning('Error checking GPU support. Falling back to CPU encoding.')
            return False