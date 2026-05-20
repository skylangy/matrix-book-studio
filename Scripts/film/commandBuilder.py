from commandBuilderContext import CommandBuilderContext
from typing import List
from filmSettings import FilmSettings
from track import Track
from logger import Logger
import subprocess

class CommandBuilder:
    def __init__(self, tracks: List[Track]):
        self.tracks = tracks
        self.logger = Logger(self.__class__.__name__)
        self.ffmpeg_bin = self._find_ffmpeg()

    def build(self, context: CommandBuilderContext) -> List[str]:
        cmd = [self.ffmpeg_bin, '-hide_banner', '-loglevel', 'error']

        if context.overwrite:
            cmd.append('-y')

        filter_complex = []

        for track in self.tracks:
            # Collect input arguments
            inputs = track.get_inputs(context)
            cmd.extend(inputs)

            # Collect filter chain
            track.get_filter(context)

            if track.has_input:
                # context.inputs.extend(inputs)
                context.input_index += 1

            # if not track.is_overlay:
            #     context.pre_track = track

        self._build_inputs(cmd, context)
        self._build_connects(filter_complex, context)
        self._build_overlays(filter_complex, context)
        self._build_audio(filter_complex, context)

        filter_complex = self._clean(filter_complex)
        self.print_values('Filter complex:', filter_complex)

        if filter_complex:
            cmd.append('-filter_complex')
            cmd.append(';'.join(filter_complex))

        if context.current_label:
            cmd.extend(['-map', str(context.current_label)])

        if context.current_audio_label:
            cmd.extend(['-map', str(context.current_audio_label)])

        cmd.extend([
            '-c:v', context.settings.video_codec,
            '-pix_fmt', context.settings.pixel_format,
            '-r', str(context.settings.framerate),
            '-c:a', context.settings.audio_codec,
            '-t',  str(context.settings.duration),
            context.output
        ])

        return cmd

    def _find_ffmpeg(self) -> str:
        '''Find FFmpeg executable or raise an error if not found.'''
        try:
            result = subprocess.run(['ffmpeg', '-version'], capture_output=True, text=True, check=True)
            self.logger.info('FFmpeg found: %s', result.stdout.split('\n')[0])
            return 'ffmpeg'
        except (subprocess.CalledProcessError, FileNotFoundError):
            self.logger.error("FFmpeg not found. Please install FFmpeg and ensure it's in your PATH.")
            raise FileNotFoundError("FFmpeg is required but not found.")

    def _build_inputs(self, cmd: List[str], context: CommandBuilderContext):
        self.logger.info(f'Building inputs: {context.current_label}')

        cmd.extend(context.inputs)
        self.print_values('Inputs:', context.inputs)
        self.logger.info('')

    def _build_connects(self, filter_complex: List[str], context: CommandBuilderContext):
        self.logger.info(f'Building connects: {context.current_label}')

        pre_track: Track = None
        transitions = []
        concats = []
        end_label = None

        for track in context.connect_tracks:
            track_filter= f'{track.filter_info.input_label}{','.join(track.filter_info.filters)}{track.filter_info.output_label}'
            self.logger.info(f'Track filter: {track_filter}')
            filter_complex.append(track_filter)

            # handle transition
            if pre_track:
                current_label = end_label if end_label else pre_track.filter_info.output_label if pre_track else ''
                end_label = f'[xf{track.filter_info.input_index}]'
                if pre_track.transition:
                    offset = pre_track.start + pre_track.duration - pre_track.transition_duration
                    transition_filter = (
                        f'{current_label}{track.filter_info.output_label}xfade=transition={pre_track.transition}'
                        f':duration={pre_track.transition_duration}:offset={offset}'
                        f'{end_label}'
                        )
                else:
                    concats.append(track.filter_info.output_label)
                    transition_filter = None

                if transition_filter:
                    transitions.append(transition_filter)
            else:
                if not track.transition:
                    concats.append(track.filter_info.output_label)

            pre_track = track

        self.print_values('Transitions:', transitions)
        self.print_values('Concats:', concats)

        filter_complex.extend(transitions)

        if concats:
            concat_filter = f'{"".join(concats)}concat=n={len(concats)}:v=1:a=0{end_label}'
            filter_complex.append(concat_filter)
            self.logger.info(f'Concat filter: {concat_filter}')

        context.current_label = end_label
        self.print_values(f'Filter complex after connects: {end_label}', filter_complex)
        self.logger.info('')

    def _build_overlays(self, filter_complex: List[str], context: CommandBuilderContext):
        self.logger.info(f'Building overlays: {context.current_label}')

        end_label = context.current_label
        filters = []

        for track in context.overlays_tracks:
            start_label = track.filter_info.input_label if track.has_input else end_label
            self.logger.info(f'Start label: {start_label}, End label: {end_label}, Current label: {context.current_label}')
            track_filter = f'{start_label}{';'.join(track.filter_info.filters)}{track.filter_info.output_label}'\
                            .replace('{context.current_label}', context.current_label)
            filters.append(track_filter)
            end_label = track.filter_info.output_label
            context.current_label = end_label

        filter_complex.extend(filters)
        context.current_label = end_label

        self.print_values('Overlay filters:', filters)
        self.logger.info('')

    def _build_audio(self, filter_complex: List[str], context: CommandBuilderContext):
        """Build audio tracks and add them to the command."""
        self.logger.info(f'Building audio tracks: {context.current_label}')

        filters = []
        current_label = ''

        for track in context.audio_tracks:
            self.logger.info(f'Processing audio track: {track}')
            track_filter = f'{track.filter_info.input_label}{",".join(track.filter_info.filters)}{track.filter_info.output_label}'
            filters.append(track_filter)
            current_label = track.filter_info.output_label

            self.logger.info(f'Audio filter: {track_filter}')

        filter_complex.extend([f'{",".join(filters)}'])
        context.current_audio_label = current_label


        self.print_values('Audio end label:', [current_label])

    def _clean(self, list: List[str]) -> List[str]:
        return [item for item in list if item.strip()]

    def print_values(self, message: str, values: List) -> None:
        self.logger.info(message)
        for value in values:
            self.logger.info(f'\t - {value}')
        self.logger.info('')