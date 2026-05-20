import whisper
import argparse
import torch
from opencc import OpenCC
import re

class SrtLine:
    """Represents a single SRT entry with index, timestamps, and text."""
    def __init__(self, index, start_time, end_time, text):
        self.index = index
        self.start_time = start_time
        self.end_time = end_time
        self.text = text.strip()

    def merge_with(self, other):
        """Merge this line with another SrtLine."""
        self.text += " " + other.text
        self.end_time = other.end_time

    def __str__(self):
        """String representation of the SRT entry."""
        return f"{self.index}\n{self.start_time} --> {self.end_time}\n{self.text}\n"

class SrtFile:
    """Represents an entire SRT file containing a list of SrtLine instances."""
    def __init__(self, lines=None):
        self.lines = lines if lines is not None else []
        self.word_count = 0

    def add_line(self, line: SrtLine):
        """Add a single SrtLine to the file."""
        self.lines.append(line)
        self.word_count += len(line.text)

    def __str__(self):
        """String representation of the entire SRT file."""
        return '\n'.join(str(line) for line in self.lines) + '\n'

    def load(self, file_path:str):
        """Load SRT file from disk."""
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            lines = content.split('\n\n')
            for block in lines:
                if not block.strip():
                    continue
                parts = block.strip().split('\n', 2)
                if len(parts) < 3:
                    continue
                index_str = parts[0].strip()
                if index_str.startswith('\ufeff'):
                    index_str = index_str[1:]
                index = int(index_str)
                time_range = parts[1].split(' --> ')
                start_time = time_range[0]
                end_time = time_range[1]
                text = parts[2].strip()
                self.add_line(SrtLine(index, start_time, end_time, text))

    def save(self, file_path):
        """Save the SRT file to disk with UTF-8 BOM."""
        with open(file_path, 'w', encoding='utf-8-sig') as f:
            f.write(str(self))

    def optimize(self, min_len=10, max_len=80):
        """Optimize the SRT file by merging short lines."""
        optimized = []
        i = 0
        while i < len(self.lines):
            current = self.lines[i]
            curr_len = len(current.text)

            # Lookahead to next line
            if i + 1 < len(self.lines):
                next_line = self.lines[i + 1]
                next_len = len(next_line.text)

                if curr_len < min_len and (curr_len + next_len) < max_len:
                    # Merge current and next line
                    merged_text = current.text + ' ' + next_line.text
                    merged_line = SrtLine(
                        index=0,  # temporary index
                        start_time=current.start_time,
                        end_time=next_line.end_time,
                        text=merged_text.strip()
                    )
                    optimized.append(merged_line)
                    i += 2
                    continue

            # No merge, keep as-is
            optimized.append(SrtLine(
                index=0,  # temporary index
                start_time=current.start_time,
                end_time=current.end_time,
                text=current.text
            ))
            i += 1

        # 🔁 Renumber indexes
        for idx, line in enumerate(optimized, start=1):
            line.index = idx

        self.lines = optimized

    def text_content(self):
        """Return the combined text content of all lines."""
        return ' '.join(line.text for line in self.lines)

class LineProcessor:
    def __init__(self):
        self.converter = OpenCC('t2s')

    def process(self, line: str) -> str:
        """Process a line of text."""
        # Placeholder for any line processing logic
        line = line.strip()
        line = self.converter.convert(line)  # Convert Traditional to Simplified Chinese
        line = re.sub(r'\s+', ' ', line).replace(',', '，')

        return line

class Video:
    def __init__(self,file: str, language = 'zh'):
        self.file = file
        self.model = whisper.load_model('turbo').to("cuda" if torch.cuda.is_available() else "cpu")
        self.language = language
        self.line_processor = LineProcessor()

    def transcribe(self, srt_path: str) -> None:
        result = self.model.transcribe(self.file,language=self.language, verbose=True, word_timestamps=True)

        srt_file = SrtFile()
        for i, segment in enumerate(result["segments"], 1):
            start_time = self.format_time(segment["start"])
            end_time = self.format_time(segment["end"])
            text = segment["text"].strip()
            if text:
                simplified_text = self.line_processor.process(text)
                srt_line = SrtLine(i, start_time, end_time, simplified_text)
                srt_file.add_line(srt_line)
        srt_file.save(srt_path)

    def format_time(self, seconds):
        """Convert seconds to SRT time format."""
        ms = int((seconds % 1) * 1000)
        seconds = int(seconds)
        minutes = seconds // 60
        hours = minutes // 60
        seconds = seconds % 60
        minutes = minutes % 60
        return f"{hours:02d}:{minutes:02d}:{seconds:02d},{ms:03d}"

if __name__ == "__main__":
    # py srt.py --video "D:\Videos\逻辑与生活\逻辑与生活-华为电脑能干啥.mov" --srt "D:\Videos\逻辑与生活\逻辑与生活-华为电脑能干啥.srt"
    parser = argparse.ArgumentParser(description="Transcribe subtitle from video file.")
    parser.add_argument("--video", help="Path to the video file", type=str, nargs='?')
    parser.add_argument("--srt", help="Path to the output SRT file", type=str, nargs='?')
    args = parser.parse_args()

    print(f'Starting transcription for video: {args.video}')

    video = Video(args.video)
    video.transcribe(args.srt)

    print(f"Transcription completed. SRT file saved to {args.srt}")
