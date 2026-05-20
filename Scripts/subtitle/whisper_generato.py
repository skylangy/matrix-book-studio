

import whisper
import srt
from datetime import timedelta
from base_generator import BaseSubtitleGenerator

class WhisperSubtitleGenerator(BaseSubtitleGenerator):
    def __init__(self, model_name="base"):
        self.model = whisper.load_model(model_name)
        self.log(f"Loaded Whisper model: {model_name}")

    def generate(self, mp3_path, script_path=None, output_path="output.srt"):
        self.log(f"Transcribing audio file: {mp3_path}")

        # Transcribe the audio file using Whisper
        result = self.model.transcribe(mp3_path)

        # Extract segments (each segment corresponds to a subtitle)
        segments = result['segments']

        # Convert segments into SRT subtitles
        subtitles = []
        for i, segment in enumerate(segments):
            start_time = segment['start']
            end_time = segment['end']
            content = segment['text'].strip()

            subtitle = srt.Subtitle(index=i+1,
                                    start=timedelta(seconds=start_time),
                                    end=timedelta(seconds=end_time),
                                    content=content)
            subtitles.append(subtitle)

        # Write the subtitles to an SRT file
        self.write_srt(subtitles, output_path)

        # Optionally, if a script is provided, you can align it with the generated subtitles
        if script_path:
            script = self.load_content(script_path)
            lines = self.split_lines(script)
            # Additional alignment logic if required

        self.log(f"Subtitle generation completed and saved to {output_path}")
