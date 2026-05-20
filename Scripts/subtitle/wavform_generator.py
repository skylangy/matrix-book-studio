from base_generator import BaseSubtitleGenerator

from datetime import timedelta
from pydub import AudioSegment
import matplotlib.pyplot as plt
import librosa
import numpy as np
import srt


class WavFormSubtitleGenerator(BaseSubtitleGenerator):
    def load_audio(self, mp3_path):
        self.log(f"Loading audio file from {mp3_path}")
        audio, sr = librosa.load(mp3_path, sr=None)
        return audio, sr

    def analyze_audio(self, audio, sr, threshold=0.01):
        self.log("Analyzing audio for voice and silence segments")
        energy = librosa.feature.rms(y=audio)[0]
        frames = np.nonzero(energy > threshold)[0]
        times = librosa.frames_to_time(frames, sr=sr)
        return times

    def generate(self, mp3_path, script_path, output_path):
        audio, sr = self.load_audio(mp3_path)
        times = self.analyze_audio(audio, sr)
        script = self.load_content(script_path)
        lines = self.split_lines(script)

        subtitles = []
        num_segments = min(len(times) - 1, len(lines))
        for i in range(num_segments):
            start_time = times[i]
            end_time = times[i + 1] if i + 1 < len(times) else (len(audio) / sr)
            subtitle = srt.Subtitle(index=i+1, start=timedelta(seconds=start_time),
                                    end=timedelta(seconds=end_time), content=lines[i])
            subtitles.append(subtitle)

        self.write_srt(subtitles, output_path)

        self.plot_analysis(times, audio, sr)

    def plot_analysis(self, times, audio, sr):
        self.log("Plotting audio analysis result")
        plt.figure(figsize=(14, 5))

        # Plot the waveform
        librosa.display.waveshow(audio, sr=sr, alpha=0.6)

        # Plot vertical lines for voice segments
        # plt.vlines(times, ymin=min(audio), ymax=max(audio), color='r', linestyle='--', label='Voice segments')

        # Add labels and title
        plt.xlabel('Time (s)')
        plt.ylabel('Amplitude')
        plt.title('Audio Waveform and Voice Segments')

        # Add a legend
        plt.legend()

        # Adjust layout and show plot
        plt.tight_layout()
        plt.show()