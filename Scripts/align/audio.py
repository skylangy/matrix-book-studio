import wave
import json
from vosk import Model, KaldiRecognizer
from pydub import AudioSegment, silence
import librosa
import librosa.display
import matplotlib.pyplot as plt
import numpy as np

class Audio:

    def convert_mp3_to_wav(self, input_file, output_file, channel=1, sample_rate=16000):
        """
        Converts an MP3 file to a WAV file.

        :param input_file: Path to the input MP3 file.
        :param output_file: Path to the output WAV file.
        """

        audio = AudioSegment.from_mp3(input_file)
        audio = audio.set_channels(channel)
        audio = audio.set_frame_rate(sample_rate)
        audio.export(output_file, format="wav")

    def detect_sound_segments(self, wav_file, silence_thresh=-40, min_silence_len=200):
        """
        Detects sound and silence segments from a WAV file.

        :param wav_file: Path to WAV file.
        :param silence_thresh: Silence threshold in dBFS.
        :param min_silence_len: Minimum length of silence to consider (in ms).
        :return: List of (start_sec, end_sec) tuples where sound is detected.
        """
        audio = AudioSegment.from_wav(wav_file)
        non_silent_ranges = silence.detect_nonsilent(audio,
                                                     min_silence_len=min_silence_len,
                                                     silence_thresh=silence_thresh)

        # Convert milliseconds to seconds
        return [(start / 1000.0, end / 1000.0) for start, end in non_silent_ranges]

    def generate_waveform_image(self, wav_file, output_image="waveform.png",
                                 start_sec=None, end_sec=None, dpi=300):
        """
        Generates a waveform image from a WAV file, optionally for a trimmed time range.

        :param wav_file: Path to WAV file.
        :param output_image: Path to output image file (e.g. waveform.png).
        :param start_sec: Start time in seconds (optional).
        :param end_sec: End time in seconds (optional).
        :param dpi: Image resolution (dots per inch).
        """
        y, sr = librosa.load(wav_file, sr=None)  # Keep original sample rate

        # Trim by time range if specified
        if start_sec is not None or end_sec is not None:
            start_sample = int(start_sec * sr) if start_sec else 0
            end_sample = int(end_sec * sr) if end_sec else len(y)
            y = y[start_sample:end_sample]

        # Generate plot
        plt.figure(figsize=(12, 4), dpi=dpi)
        librosa.display.waveshow(y, sr=sr, alpha=0.8)
        plt.title(f"Waveform ({start_sec or 0:.2f}s to {end_sec or len(y)/sr:.2f}s)")
        plt.xlabel("Time (s)")
        plt.ylabel("Amplitude")
        plt.tight_layout()
        plt.savefig(output_image)
        plt.close()

    def detect_sentences(self, wav_file, silence_thresh_db=-40, min_silence_len_ms=300, frame_length=2048, hop_length=512):
        """
        Detects sentence-like audio segments separated by silence.

        :param wav_file: Path to WAV file.
        :param silence_thresh_db: Threshold in dB for silence detection.
        :param min_silence_len_ms: Minimum silence duration between sentences (in ms).
        :param frame_length: Size of analysis window.
        :param hop_length: Step between windows.
        :return: List of (start_sec, end_sec) for sentence-like regions.
        """
        y, sr = librosa.load(wav_file, sr=None)

        # Compute RMS energy
        rms = librosa.feature.rms(y=y, frame_length=frame_length, hop_length=hop_length)[0]
        times = librosa.frames_to_time(np.arange(len(rms)), sr=sr, hop_length=hop_length)

        # Convert threshold to linear
        silence_thresh = librosa.db_to_amplitude(silence_thresh_db)

        segments = []
        is_speech = rms > silence_thresh
        start = None

        for i, speech in enumerate(is_speech):
            t = times[i]
            if speech:
                if start is None:
                    start = t
            else:
                if start is not None:
                    duration = t - start
                    if duration > 0.2:  # Filter out short blips
                        segments.append((start, t))
                    start = None

        # Handle trailing audio
        if start is not None:
            segments.append((start, times[-1]))

        # Merge segments separated by short silence
        merged = []
        last_end = None
        for start, end in segments:
            if not merged:
                merged.append((start, end))
            else:
                gap = start - merged[-1][1]
                if gap < (min_silence_len_ms / 1000.0):
                    # Merge with previous
                    merged[-1] = (merged[-1][0], end)
                else:
                    merged.append((start, end))

        return merged

    def transcribe_with_timestamps(self, wav_file, model_path):
        """
        Transcribes a WAV file and extracts word-level timestamps using Vosk.

        :param wav_file: Path to the WAV file.
        :param model_path: Path to the Vosk model directory.
        :return: List of words with timestamps.
        """
        model = Model(model_path)
        wf = wave.open(wav_file, "rb")
        rec = KaldiRecognizer(model, wf.getframerate())
        rec.SetWords(True)

        results = []
        while True:
            data = wf.readframes(4000)
            if len(data) == 0:
                break
            if rec.AcceptWaveform(data):
                results.append(json.loads(rec.Result()))
        results.append(json.loads(rec.FinalResult()))

        words = []
        for res in results:
            if 'result' in res:
                words.extend(res['result'])

        return words

    def get_speech_segments(self, text_file_path: str) -> list:
        """
        Count speech-like segments in a Chinese text file.
        A segment is split by Chinese punctuation marks that indicate pauses.

        :param text_file_path: Path to the input .txt file.
        :return: Number of speech segments.
        """
        import re

        with open(text_file_path, "r", encoding="utf-8") as f:
            content = f.read()

        # Remove extra whitespace and normalize
        content = content.strip()

        # Split on Chinese sentence/pause punctuation marks
        segments = re.split(r'[，。！？；：,.!?;:]', content)

        # Filter out empty or whitespace-only segments
        segments = [seg.strip() for seg in segments if seg.strip()]

        return segments

    def merge_text_segments(self, text_segments, target_count):
        """
        Merge text segments to match the target segment count efficiently.

        :param original_text_segments: List of original short text segments (194 segments).
        :param target_count: Target number of segments after merging (145 segments).
        :return: List of merged text segments.
        """
        import math

        total_segments = len(text_segments)
        if total_segments <= target_count:
            return text_segments

        ideal_group_size = total_segments / target_count
        merged_segments = []
        buffer = ""
        buffer_count = 0

        for i, seg in enumerate(text_segments):
            buffer += seg
            buffer_count += 1

            # Flush if we hit average size and a punctuation boundary
            is_end = seg[-1] in "。！？"  # use only full sentence enders for merging
            remaining_merges = target_count - len(merged_segments) - 1
            remaining_texts = total_segments - i - 1

            if (buffer_count >= math.ceil(ideal_group_size) and is_end) or remaining_merges >= remaining_texts:
                merged_segments.append(buffer.strip())
                buffer = ""
                buffer_count = 0

        # Add any leftovers
        if buffer:
            merged_segments.append(buffer.strip())

        # Fix if we merged too little
        while len(merged_segments) > target_count:
            merged_segments[-2] += merged_segments[-1]
            merged_segments.pop()

        return merged_segments

    def generate_srt(self, audio_segments: list, text_segments: list, output_path: str):
        """
        Generate an SRT subtitle file using audio timing and text segments.

        :param audio_segments: List of (start, end) time tuples in seconds.
        :param text_segments: List of corresponding text segments.
        :param output_path: Path to save the .srt file.
        """
        from datetime import timedelta

        def format_time(seconds):
            td = timedelta(seconds=seconds)
            total_seconds = int(td.total_seconds())
            milliseconds = int((td.total_seconds() % 1) * 1000)
            return f"{str(timedelta(seconds=total_seconds))},{milliseconds:03d}"

        if len(text_segments) < len(audio_segments):
            raise ValueError("More audio segments than text segments — cannot align.")

        # Merge text segments to match audio segment count
        merged_text_segments = self.merge_text_segments(text_segments, len(audio_segments))

        # Generate SRT file
        with open(output_path, 'w', encoding='utf-8') as f:
            for i, ((start, end), text) in enumerate(zip(audio_segments, merged_text_segments), 1):
                f.write(f"{i}\n")
                f.write(f"{format_time(start)} --> {format_time(end)}\n")
                f.write(f"{text}\n\n")

if __name__ == "__main__":
    folder = r"I:\Audio Books\赵旭\一个69届初中生的文革十年"
    file = r"一个69届初中生的文革十年-第一章　风萧萧大年三十与父母一同插队落户积石山"
    mp3 = rf"{folder}/mp3/{file}.mp3"
    wav = rf"{folder}/wav/{file}.wav"
    txt = rf"{folder}/txt/{file}.txt"
    srt = rf"{folder}/mp4/{file}.srt"

    audio = Audio()
    # audio.convert_mp3_to_wav(mp3, wav)
    # words = audio.transcribe_with_timestamps(wav, "vosk-model-small-cn-0.22")

    # for w in words:
    #     print(f"{w['word']} — {w['start']:.2f}s to {w['end']:.2f}s")
    segments = audio.detect_sentences(mp3)

    for start, end in segments:
        print(f"Sentence: {start:.2f}s → {end:.2f}s")

    print(f'Total segments: {len(segments)}')

    audio.generate_waveform_image(wav, "waveform.png", 0, 10)

    speech_segments = audio.get_speech_segments(txt)
    print(f'Total segments in text file: {len(speech_segments)}')
    for i, seg in enumerate(speech_segments):
        print(f"[{i+1}] {seg}")

    merged_segments = audio.merge_text_segments(speech_segments, len(segments))
    for i, seg in enumerate(merged_segments):
        print(f"[{i+1}] {seg}")

    audio.generate_srt(segments, merged_segments, srt)
