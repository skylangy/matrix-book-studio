

from base_generator import BaseSubtitleGenerator
from datetime import timedelta
from pydub import AudioSegment

import srt

class DurationSubtitleGenerator(BaseSubtitleGenerator):
    CHARS_TO_IGNORE = ["¿", "¡", '""', "%", '"', "�", "ʿ", "·", "჻", "~", "՞",
                  "؟", "،", "।", "॥", "«", "»", "„", "“", "”", "「", "」", "‘", "’", "《", "》", "(", ")", "[", "]",
                  "{", "}", "=", "`", "_", "+", "<", ">", "–", "°", "´", "ʾ", "‹", "›", "©", "®", "—", "→",
                   "﹂", "﹁", "‧", "～", "﹏",  "｛", "｝", "（", "）", "［", "］", "【", "】", "‥", "〽",
                  "『", "』", "〝", "〟", "⟨", "⟩", "〜", "♪", "؛", "/", "\\", "º", "−", "^", "'", "ʻ", "ˆ"]

    def __init__(self) -> None:
        super().__init__()
        self.verbose = False

        end_duration = 0.6
        pause_duration = 0.3

        self.silence_words = set(self.CHARS_TO_IGNORE)
        #[' ', '\'', '-', '.', '“', '”', '•', '；', '：', '（ ', '）',"［", "］", "【", "】","『", "』"]
        self.symbol_durations = {
            ',': pause_duration,
            '、': pause_duration,
            ':': pause_duration,
            '，': pause_duration,
            '：': pause_duration,

            ';': end_duration,
            '.': end_duration,
            '。': end_duration,
            '?': end_duration,
            '!': end_duration,
            '；': end_duration,
            '？': end_duration,
            '！': end_duration,

        }
        self.use_symbol_durations = True

    def generate(self, mp3_path, script_path, output_path):
        self.logger.debug(f"Generating subtitle for {mp3_path} and {script_path} to {output_path}")

        script_content = self.load_content(script_path)
        script_content = self.preprocess_text(script_content)

        lines = self.split_lines(script_content)
        num_characters_speakable = self.calculate_pronouncable_characters(script_content)
        audio_duration = self.get_audio_duration(mp3_path)
        avg_duration_per_char = self.calculate_average_duration(audio_duration, lines)

        subtitles = []
        start_time = timedelta(seconds=0)

        for line in lines:
            line_duration = self.calculate_line_duration(line, avg_duration_per_char)
            end_time = start_time + timedelta(seconds=line_duration)
            subtitles.append(srt.Subtitle(index=len(subtitles) + 1, start=start_time, end=end_time, content=line.strip()))
            start_time = end_time

        self.logger.debug("-" * 80)
        self.logger.debug(f'Audio duration:             {self.format_time(audio_duration)}')
        self.logger.debug(f'Content length:             {len(script_content)}')
        self.logger.debug(f'Pronouncable characters:    {num_characters_speakable}')
        self.logger.debug(f"Number of lines:            {len(lines)}")
        self.logger.debug(f"Average character duration: {self.format_time(avg_duration_per_char)}")
        self.logger.debug("-" * 80)

        self.write_srt(subtitles, output_path)

        srt_content = self.load_content(output_path)
        success, message = self.verify_srt(srt_content)
        if success:
            self.logger.info(f"Subtitle generated successfully: {message}")
        else:
            self.logger.error(f"Verification failed: {message}")

    def preprocess_text(self, text) -> str:
        return text.replace('“', '')\
                    .replace('”', '')\
                    .replace('\n', '')\
                    .replace('《', '')\
                    .replace('》', '')\

    def get_audio_duration(self, file_path) -> float:
        '''Get the duration of the audio file in seconds'''
        audio = AudioSegment.from_file(file_path)
        return len(audio) / 1000

    def calculate_average_duration(self, total_duration, lines) -> float:
        '''Calculate the average duration per character'''
        # return total_duration / num_characters if num_characters else 0
        total_pronounced_chars = 0
        total_pause_duration = 0.0

        for line in lines:
            for char in line:
                if char in self.symbol_durations:
                    total_pause_duration += self.symbol_durations[char]
                elif char not in self.silence_words:
                    total_pronounced_chars += 1

        # Calculate the remaining duration for the pronounced characters
        remaining_duration = total_duration - total_pause_duration
        duration_per_char = remaining_duration / total_pronounced_chars

        self.logger.debug(f"Total duration:                 {self.format_time(total_duration)}")
        self.logger.debug(f"Total pause duration:           {self.format_time(total_pause_duration)}")
        self.logger.debug(f"Total pronounced characters:    {total_pronounced_chars}")
        self.logger.debug(f"Duration per character:         {self.format_time(duration_per_char)}")

        return duration_per_char

    def calculate_pronouncable_characters(self, text):
        return sum(1 for char in text if char not in self.silence_words)

    def calculate_line_duration(self, line, avg_duration_per_char):
        line_duration = 0
        for char in line.strip():
            if char in self.silence_words:
                if self.verbose:
                    self.logger.debug(f"Skipping {char}")
                continue
            duration = avg_duration_per_char
            if self.use_symbol_durations and char in self.symbol_durations:
                duration = self.symbol_durations[char] # avg_duration_per_char * self.symbol_durations[char]
            line_duration += duration

        if self.verbose:
            self.logger.debug(f"Duration: [{self.format_time(line_duration)}] {line}")

        return line_duration

    def format_time(self, seconds):
        td = timedelta(seconds=seconds)
        return f"{str(td)}"
