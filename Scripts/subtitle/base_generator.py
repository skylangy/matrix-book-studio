
from abc import ABC, abstractmethod
from datetime import datetime, timedelta
import srt
import re
from logger import Logger

class BaseSubtitleGenerator(ABC):
    def __init__(self) -> None:
        self.logger = Logger(self.__class__.__name__)
        self.log_subtitles = False

    def load_content(self, file_path) -> str:
        self.logger.debug(f"Loading script content from {file_path}")

        with open(file_path, 'r', encoding="utf-8") as file:
            script = file.read()
            return script

    def split_lines(self, text: str) -> list[str]:
        split_lines = re.split(r'([。？！；])', text)

        lines = [split_lines[i] + split_lines[i + 1] for i in range(0, len(split_lines) - 1, 2)]

        if len(split_lines) % 2 != 0:
            lines.append(split_lines[-1])

        trimmed_lines = [line.strip() for line in lines if line.strip()]

        return trimmed_lines

    def write_srt(self, subtitles, output_path):
        self.logger.debug(f"Writing {len(subtitles)} subtitles to {output_path}")
        with open(output_path, "w", encoding='utf-8') as f:
            f.write(srt.compose(subtitles))

        if self.log_subtitles:
            self.print_subtitles(subtitles)

    def print_subtitles(self, subtitles):
        for sub in subtitles:
           print(self.print_subtitle(sub))

    def print_subtitle(self, subtitle):
        start_str = self.format_timedelta(subtitle.start)
        end_str = self.format_timedelta(subtitle.end)
        return f"{subtitle.index}\n{start_str} --> {end_str}\n{subtitle.content}\n"

    def verify_srt(self, srt_content):
        # Regular expressions to match SRT components
        index_pattern = re.compile(r"^\d+$")
        timestamp_pattern = re.compile(r"^\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}$")

        # Split into blocks (each block represents a subtitle)
        blocks = srt_content.strip().split("\n\n")

        previous_end_time = None

        for block in blocks:
            lines = block.strip().split("\n")
            if len(lines) < 3:
                return False, "Block does not have enough lines."

            # Check index
            index = lines[0]
            if not index_pattern.match(index):
                return False, f"Invalid index: {index}"

            # Check timestamp
            timestamp = lines[1]
            if not timestamp_pattern.match(timestamp):
                return False, f"Invalid timestamp: {timestamp}"

            start_time_str, end_time_str = timestamp.split(" --> ")
            start_time = datetime.strptime(start_time_str, "%H:%M:%S,%f")
            end_time = datetime.strptime(end_time_str, "%H:%M:%S,%f")

            if start_time >= end_time:
                return False, "Start time is not before end time."

            if previous_end_time and start_time < previous_end_time:
                return False, "Subtitles overlap."

            previous_end_time = end_time

        return True, "SRT file is valid."

    @staticmethod
    def format_timedelta(td):
        total_seconds = int(td.total_seconds())
        milliseconds = int((td.total_seconds() - total_seconds) * 1000)
        return f"{str(timedelta(seconds=total_seconds))},{milliseconds:03}"

    @abstractmethod
    def generate(self, mp3_path, script_path, output_path):
        pass
