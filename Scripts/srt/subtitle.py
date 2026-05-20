import whisper
import os
from opencc import OpenCC  # For converting Traditional to Simplified Chinese
import re
import Levenshtein
import logging
import torch
import time
import argparse

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

class ScheduleItem:
    """Represents a single schedule item with MP3 and SRT folder paths."""
    def __init__(self, mp3_folder, srt_folder):
        self.mp3_folder = mp3_folder
        self.srt_folder = srt_folder

    def __str__(self):
        return f"MP3 Folder: {self.mp3_folder}, SRT Folder: {self.srt_folder}"

class ScheduleFile:
    """Represents a schedule file with MP3 and SRT folder paths."""
    def __init__(self, file:str):
        self.file = file
        self.schedule_items = self.load()

    def load(self):
        """Load schedule items from the file."""
        if not os.path.exists(self.file):
            raise FileNotFoundError(f"Schedule file not found: {self.file}")

        print(f"Loading schedule from {self.file}")
        items = []
        with open(self.file, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if not line:
                    continue
                parts = line.split(',')
                if len(parts) != 2:
                    print(f"Invalid schedule entry: {line}")
                    continue
                mp3_folder, srt_folder = parts
                items.append(ScheduleItem(mp3_folder.strip(), srt_folder.strip()))

        print(f"Loaded {len(items)} schedule items.")
        return items

    def __str__(self):
        return f"MP3 Folder: {self.mp3_folder}, SRT Folder: {self.srt_folder}"

class Scheduler:
    """Handles the scheduling of MP3 and SRT folder processing."""
    def __init__(self, schedule_file:str):
        self.schedule_file = ScheduleFile(schedule_file)

    def start(self):
        """Start processing the schedule."""
        print(f"Starting scheduled processing for {len(self.schedule_file.schedule_items)} items.")

        processed_count = 0
        generator = WhisperSrtGenerator()
        for item in self.schedule_file.schedule_items:
            print(f"Processing scheduled entry: MP3 folder: {item.mp3_folder}, SRT folder: {item.srt_folder}")
            if not os.path.exists(item.mp3_folder):
                print(f"MP3 folder does not exist: {item.mp3_folder}")
                continue

            # Ensure the SRT folder exists
            if not os.path.exists(item.srt_folder):
                os.makedirs(item.srt_folder)

            mp3_count = len([f for f in os.listdir(item.mp3_folder) if f.lower().endswith('.mp3')])
            srt_count = len([f for f in os.listdir(item.srt_folder) if f.lower().endswith('.srt')])

            if mp3_count == srt_count:
                print(f"Skipping folder {item.mp3_folder} as MP3 and SRT file counts are equal ({mp3_count}).")
                continue

            generator.process_folder(item.mp3_folder, item.srt_folder)
            processed_count += 1

            print(f"Completed processing for {item.mp3_folder} -> {item.srt_folder}")
            print("=" * 80)
            print("\n")

        print(f"{processed_count} scheduled items processed.")

class SubtitleFixer:
    def __init__(self, whisper_srt_file, correct_text_file, fixed_srt_file):
        """Initialize with file paths."""
        self.whisper_srt_file = whisper_srt_file
        self.correct_text_file = correct_text_file
        self.fixed_srt_file = fixed_srt_file
        self.whisper_srt = None  # Will hold an SrtFile instance
        self.correct_lines = []

    def parse_srt(self, srt_file):
        """Parse an SRT file into an SrtFile instance."""
        srt = SrtFile()
        with open(srt_file, 'r', encoding='utf-8') as f:
            content = f.read().strip()
            if content.startswith('\ufeff'):
                content = content[1:]
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
                srt.add_line(SrtLine(index, start_time, end_time, text))
        return srt

    def normalize_text(self, text):
        text = re.sub(r'[《》“”.,!?;，。！？；]', '', text)
        return text

    def break_text(self, text):
        # Split text into lines by punctuation marks such as ., !, ?, and commas
        sentences = re.split(r'[.,?!]\s*', text)
        sentences = [s for s in sentences if s]
        return sentences

    def load_files(self):
        """Load Whisper SRT and correct text into memory."""
        if not os.path.exists(self.whisper_srt_file):
            raise FileNotFoundError(f"Whisper SRT file not found: {self.whisper_srt_file}")
        if not os.path.exists(self.correct_text_file):
            raise FileNotFoundError(f"Correct text file not found: {self.correct_text_file}")

        self.whisper_srt = self.parse_srt(self.whisper_srt_file)
        print(f"Loaded {len(self.whisper_srt.lines)} entries from Whisper SRT with {self.whisper_srt.word_count} words")

        with open(self.correct_text_file, 'r', encoding='utf-8') as f:
            text = f.read().strip()
            correct_segments = text.split("。")
            correct_segments = [s.strip() + "。" for s in correct_segments if s]

            correct_lines = []
            for line in self.whisper_srt.lines:
                clean_incorrect = self.normalize_text(line.text)  # Normalize input
                best_match = min(correct_segments, key=lambda x: Levenshtein.distance(clean_incorrect, x))
                correct_lines.append(best_match)

            self.correct_lines = correct_lines
            print(f"Corrected lines: {len(correct_lines)}")

        print(f"Loaded {len(self.correct_lines)} lines from correct text")

    def levenshtein_distance(self, s1, s2):
        """Calculate Levenshtein distance between two strings."""
        if len(s1) < len(s2):
            return self.levenshtein_distance(s2, s1)
        if len(s2) == 0:
            return len(s1)
        previous_row = range(len(s2) + 1)
        for i, c1 in enumerate(s1):
            current_row = [i + 1]
            for j, c2 in enumerate(s2):
                insertions = previous_row[j + 1] + 1
                deletions = current_row[j] + 1
                substitutions = previous_row[j] + (c1 != c2)
                current_row.append(min(insertions, deletions, substitutions))
            previous_row = current_row
        return previous_row[-1]

    def fine_tune(self):
        """Fix typos by matching Whisper text to correct text using Levenshtein distance."""
        fixed_srt = SrtFile()
        if len(self.whisper_srt.lines) != len(self.correct_lines):
            print(f"Warning: Segment count mismatch (Whisper: {len(self.whisper_srt.lines)}, Correct: {len(self.correct_lines)})")

        for i, entry in enumerate(self.whisper_srt.lines):
            if i < len(self.correct_lines):
                fixed_srt.add_line(SrtLine(entry.index, entry.start_time, entry.end_time, self.correct_lines[i]))
                print(f"Replaced '{entry.text}' with '{self.correct_lines[i]}'")
            else:
                print(f"Warning: No correct text for Whisper entry: {entry.text}")
                fixed_srt.add_line(entry)
        return fixed_srt

    def save_srt(self, fixed_srt):
        """Save fixed SRT to file."""
        fixed_srt.save(self.fixed_srt_file)
        print(f"Fixed SRT file generated: {self.fixed_srt_file}")

    def process(self):
        """Run the full fixing process."""
        self.load_files()
        fixed_srt = self.fine_tune()
        self.save_srt(fixed_srt)

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

class WhisperSrtGenerator:
    def __init__(self, model_name="turbo", language="zh"):
        """Initialize with Whisper model and language settings."""
        # base, small, medium
        self.logger = logging.getLogger(__name__)
        self.logger.setLevel(logging.INFO)
        console_handler = logging.StreamHandler()
        console_handler.setFormatter(logging.Formatter('[%(asctime)s] [%(levelname)s] %(message)s'))
        self.logger.addHandler(console_handler)

        self.model_name = model_name
        self.language = language

        self.logger.info(f"Initializing Whisper model: {self.model_name} for language: {self.language}")

        self.model = whisper.load_model(model_name).to("cuda" if torch.cuda.is_available() else "cpu")

        self.line_processor = LineProcessor()

    def log(self, message):
        """Log messages to console."""
        self.logger.info(message)

    def error(self, message):
        """Log error messages."""
        self.logger.error(message)

    def format_time(self, seconds):
        """Convert seconds to SRT time format."""
        ms = int((seconds % 1) * 1000)
        seconds = int(seconds)
        minutes = seconds // 60
        hours = minutes // 60
        seconds = seconds % 60
        minutes = minutes % 60
        return f"{hours:02d}:{minutes:02d}:{seconds:02d},{ms:03d}"

    def generate_srt_content(self, mp3_path) -> SrtFile:
        """Generate SRT content from an MP3 file using Whisper."""
        # Transcribe audio with timestamps
        self.log(f"Transcribing audio file: {mp3_path}")
        start = time.time()
        result = self.model.transcribe(mp3_path, language=self.language, verbose=True, word_timestamps=True)

        # Generate SRT content
        duration = time.time() - start
        self.log(f"Transcription took {duration:.2f} seconds")
        self.log(f"Transcription completed. Found {len(result['segments'])} segments.")
        srt_file = SrtFile()
        for i, segment in enumerate(result["segments"], 1):
            start_time = self.format_time(segment["start"])
            end_time = self.format_time(segment["end"])
            text = segment["text"].strip()
            if text:
                simplified_text = self.line_processor.process(text)
                srt_line = SrtLine(i, start_time, end_time, simplified_text)
                srt_file.add_line(srt_line)

        srt_file.optimize()
        return srt_file

    def process_file(self, mp3_file, output_srt_file):
        """Generate an SRT file from an MP3 file."""
        if not torch.cuda.is_available():
            self.log("CUDA is not available. Running on CPU.")
            return

        try:
            self.log(f"Start processing file: {mp3_file}")
            self.log("=" * 80)

            srt_file = self.generate_srt_content(mp3_file)
            srt_file.save(output_srt_file)

            self.log("=" * 80)
            self.log(f"Generated SRT file: {output_srt_file}")
        except Exception as e:
            self.error(f"Error: generate srt failed for file {mp3_file}, {str(e)}")

    def process_folder(self, mp3_folder, output_folder):
        """Process all MP3 files in a folder and generate SRT files."""
        if not os.path.exists(mp3_folder):
            self.log(f"Folder does not exist: {mp3_folder}")
            return

        os.makedirs(output_folder, exist_ok=True)
        mp3_files = [f for f in os.listdir(mp3_folder) if f.lower().endswith('.mp3')]
        self.log(f"Found {len(mp3_files)} MP3 files in {mp3_folder}:")

        for mp3_file in mp3_files:
            file_name = os.path.splitext(mp3_file)[0]
            output_srt_file = os.path.join(output_folder, f"{file_name}.srt")
            self.process_file(os.path.join(mp3_folder, mp3_file), output_srt_file)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Transcribe subtitle from mp3 file.")
    parser.add_argument("--mp3_file", help="Path to the mp3 file", type=str, nargs='?')
    parser.add_argument("--srt_file", help="Path to the output srt file", type=str, nargs='?')
    parser.add_argument("--mp3_folder", help="Path to the mp3 folder", type=str, nargs='?')
    parser.add_argument("--srt_folder", help="Path to the output folder", type=str, nargs='?')
    parser.add_argument("--schedule_file", help="Path to the text file with scheduled folders to transcribe", type=str, nargs='?')
    args = parser.parse_args()

    # python .\subtitle.py --mp3_folder "I:\Audio Books\曹雪芹\红楼梦\mp3" --srt_folder "I:\Audio Books\曹雪芹\红楼梦\srt"
    # python .\subtitle.py --mp3_file "I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3" --srt_file "I:\Source\封面\红楼梦\video\第一章.srt"
    # python .\subtitle.py  --schedule_file ".\schedule.txt"

    if args.mp3_file and args.srt_file:
        generator = WhisperSrtGenerator()
        generator.process_file(args.mp3_file, args.srt_file)
    elif args.mp3_folder and args.srt_folder:
        generator = WhisperSrtGenerator()
        generator.process_folder(args.mp3_folder, args.srt_folder)
    elif args.schedule_file:
        if not os.path.exists(args.schedule_file):
            print(f"Schedule file not found: {args.schedule_file}")
        else:
            scheduler = Scheduler(args.schedule_file)
            scheduler.start()
    else:
        print("Please provide either --mp3_file and --srt_file or --mp3_folder and --srt_folder or --schedule_file.")

