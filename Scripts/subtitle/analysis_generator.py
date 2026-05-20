from base_generator import BaseSubtitleGenerator
from datetime import timedelta
from pydub import AudioSegment
from pydub.silence import split_on_silence
import srt

class AnalysisSubtitleGenerator(BaseSubtitleGenerator):

    def generate(self, mp3_path, script_path, output_path):
        self.log(f"Generating subtitle for {mp3_path} and {script_path} to {output_path}")

        script_content = self.load_content(script_path)
        chunks = self.split_audio(mp3_path)
        script = self.split_text(script_content, len(chunks))

        if len(script) != len(chunks):
            raise ValueError("The number of text lines does not match the number of audio chunks")


        subtitles = []
        start_time = timedelta(seconds=0)
        self.log(f"Split audio into {len(chunks)} chunks and {len(script)} lines of script")

        for i, chunk in enumerate(chunks):
            end_time = start_time + timedelta(milliseconds=len(chunk))
            subtitles.append(srt.Subtitle(index=i+1, start=start_time, end=end_time, content=script[i].strip()))
            start_time = end_time

        self.write_srt(subtitles, output_path)

    def split_audio(self, audio_path) -> list[AudioSegment]:
        self.log(f"Splitting audio from {audio_path}")

        audio = AudioSegment.from_mp3(audio_path)
        chunks = split_on_silence(audio, min_silence_len=500, silence_thresh=-40)

        self.log(f"Split audio into {len(chunks)} chunks")
        return chunks

    def split_text(self, text, num_chunks):
        lines = text.splitlines()
        total_lines = len(lines)

        if num_chunks < total_lines:
            # If there are more lines than chunks, split lines to fit chunks
            avg_lines_per_chunk = total_lines // num_chunks
            remainder = total_lines % num_chunks

            result = []
            start = 0
            for i in range(num_chunks):
                end = start + avg_lines_per_chunk + (1 if i < remainder else 0)
                result.append('\n'.join(lines[start:end]))
                start = end
            return result
        elif num_chunks > total_lines:
            # If there are more chunks than lines, repeat the lines to fit chunks
            result = []
            while len(result) < num_chunks:
                result.extend(lines)
            return result[:num_chunks]
        else:
            # If the number of lines matches the number of chunks, return as is
            return lines
