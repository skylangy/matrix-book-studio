import torch
import pysrt
from typing import List
from datetime import timedelta

from models import SingleSegment
from alignment import Aligner
from audio import Audio
from logger import Logger

class SubtitleAligner:
    def __init__(self):
        self.language = "zh"
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.interpolate_method = "nearest"
        self.return_char_alignments = False
        self.print_progress = False
        self.align_model = None

        self.audio = Audio()
        self.aligner = Aligner()
        self.logger = Logger(self.__class__.__name__)

    def align(self, base_path, file_name):
        # align the subtitle with the audio
        self.logger.debug(f"Start processing file: '{file_name}'...")

        mp3_path = rf"{base_path}\mp3\{file_name}.mp3"
        raw_txt_path = rf"{base_path}\txt\{file_name}.txt"
        output_path = rf"{base_path}\mp4\{file_name}.srt"
        srt_path = rf"{base_path}\srt\{file_name}.srt"

        transcript = {"segments" : self.load_srt(srt_path),
                      "language": self.language}
        audio_data = self.audio.load_audio(mp3_path)

        transcript_info = [(transcript, mp3_path, output_path)]
        results = []

        self.logger.debug(f"Loading alignment model for '{self.language}', device: {self.device}...")
        align_model, align_metadata = self.aligner.load_model(self.language, self.device, model_name = self.align_model)
        for result, audio_path, output_path in transcript_info:
            # >> Align
            self.logger.debug(f"Aligning '{audio_path}' with '{srt_path}'")
            if len(transcript_info) > 1:
                input_audio = audio_path
            else:
                # lazily load audio from part 1
                input_audio = audio_data

            if align_model is not None and len(result["segments"]) > 0:
                if result.get("language", "en") != align_metadata["language"]:
                    # load new language
                    self.logger.debug(f"New language found ({result['language']})! Previous was ({align_metadata['language']}), loading new alignment model for new language...")
                    align_model, align_metadata = self.aligner.load_model(result["language"], self.device)

                self.logger.debug(f">> Performing alignment for '{audio_path}'...")
                self.logger.debug(f"   Align model: {align_model}")
                self.logger.debug(f"   Interpolate method: {self.interpolate_method}")
                self.logger.debug(f"   Reutrn char alignments: {self.return_char_alignments}")

                result = self.aligner.align(
                                result["segments"],
                                align_model,
                                align_metadata,
                                input_audio,
                                self.device,
                                interpolate_method = self.interpolate_method,
                                return_char_alignments = self.return_char_alignments,
                                print_progress = self.print_progress)
                self.logger.debug(f"Alignment for '{audio_path}' completed.")

            results.append((result, output_path))

        for item in results:
            self.convert_to_srt(item[0]['segments'], item[1])

    def load_srt(self, file_path):
        self.logger.debug(f"Loading SRT file from: {file_path}")
        srt_file = pysrt.open(file_path)
        segments: List[SingleSegment] = []

        for subtitle in srt_file:
            segment = SingleSegment(
                start=subtitle.start.ordinal / 1000.0,  # Convert milliseconds to seconds
                end=subtitle.end.ordinal / 1000.0,      # Convert milliseconds to seconds
                text=subtitle.text.replace('\n', ' ')   # Replace newline characters with spaces
            )
            segments.append(segment)

        return segments

    def convert_to_srt(self, segments, output_file):
        with open(output_file, 'w', encoding='utf-8') as srt_file:
            for i, segment in enumerate(segments, start=1):
                start_time = self.format_time(segment["start"])
                end_time = self.format_time(segment["end"])
                text = segment["text"]

                srt_file.write(f"{i}\n")
                srt_file.write(f"{start_time} --> {end_time}\n")
                srt_file.write(f"{text}\n\n")

    def format_time(self, seconds: float) -> str:
        """Converts seconds to SRT timestamp format (hh:mm:ss,ms)"""
        td = timedelta(seconds=seconds)
        hours, remainder = divmod(td.seconds, 3600)
        minutes, seconds = divmod(remainder, 60)
        milliseconds = int(td.microseconds / 1000)
        return f"{hours:02}:{minutes:02}:{seconds:02}.{milliseconds:03}"
