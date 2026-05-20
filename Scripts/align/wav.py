import librosa
import re
import numpy as np

class Audio:
    def __init__(self, audio_file_path, text_file_path, min_length=20, max_length=45):
        """Initialize the Audio class with audio and text file paths.

        Note: audio_file_path can be a WAV or MP3 file. For MP3 support, ensure ffmpeg is installed.
        """
        self.audio_file_path = audio_file_path
        self.text_file_path = text_file_path
        self.min_length = min_length
        self.max_length = max_length
        self.audio, self.sr = librosa.load(self.audio_file_path)
        self.audio_duration = librosa.get_duration(y=self.audio, sr=self.sr)
        self.text = self.load_text_file()
        self.sentences = self.split_into_sentences()

    def load_text_file(self):
        """Read the content of the text file, handling UTF-8 BOM."""
        with open(self.text_file_path, 'r', encoding='utf-8-sig') as file:
            return file.read().strip()

    def split_into_sentences(self):
        """Split text into sentences with min and max length constraints."""
        segments = re.split(r'(?<=[。！？；])', self.text)
        segments = [seg.strip() for seg in segments if seg.strip()]

        adjusted_segments = []
        i = 0
        while i < len(segments):
            current = segments[i]
            if len(current) > self.max_length:
                while len(current) > self.max_length:
                    split_pos = self.max_length
                    for j in range(self.max_length - 1, -1, -1):
                        if current[j] in '，, ':
                            split_pos = j + 1
                            break
                    adjusted_segments.append(current[:split_pos].strip())
                    current = current[split_pos:].strip()
                if current:
                    adjusted_segments.append(current)
                i += 1
            elif len(current) < self.min_length and i + 1 < len(segments):
                adjusted_segments.append((current + ' ' + segments[i + 1]).strip())
                i += 2
            else:
                adjusted_segments.append(current)
                i += 1

        final_segments = []
        i = 0
        while i < len(adjusted_segments):
            current = adjusted_segments[i]
            if len(current) < self.min_length and i + 1 < len(adjusted_segments):
                final_segments.append((current + ' ' + adjusted_segments[i + 1]).strip())
                i += 2
            else:
                final_segments.append(current)
                i += 1

        print(f"Split into {len(final_segments)} sentences")
        return final_segments

    def get_audio_duration(self):
        """Get the duration of the audio file in seconds."""
        duration = self.audio_duration
        print(f"Audio duration: {duration:.2f}s ({int(duration//3600)}:{int((duration%3600)//60):02d}:{int(duration%60):02d})")
        return duration

    def preprocess_audio(self):
        """Preprocess audio similar to Whisper's log-mel spectrogram preparation."""
        # Normalize audio amplitude (mimics Whisper's preprocessing)
        audio_normalized = self.audio / np.max(np.abs(self.audio))
        return audio_normalized

    def chunk_audio(self):
        """Chunk audio into speech segments, mimicking Whisper's 30-second chunking."""
        audio_normalized = self.preprocess_audio()

        # Detect non-silent regions (Whisper-like chunking)
        top_db = 25  # Silence threshold in dB (tuned for speech)
        non_silent = librosa.effects.split(audio_normalized, top_db=top_db, frame_length=2048, hop_length=512)

        # Convert to seconds
        speech_segments = [(start / self.sr, end / self.sr) for start, end in non_silent]

        # Merge small gaps (similar to Whisper's overlap handling)
        min_gap = 0.8  # Merge gaps shorter than 0.8 seconds
        merged_segments = []
        if not speech_segments:
            print("No speech detected; using full audio duration.")
            return [(0.0, self.audio_duration)]

        start, end = speech_segments[0]
        for next_start, next_end in speech_segments[1:]:
            if next_start - end < min_gap:
                end = next_end
            else:
                merged_segments.append((start, end))
                start, end = next_start, next_end
        merged_segments.append((start, end))

        # Debug: Print segments
        total_speech = sum(end - start for start, end in merged_segments)
        print(f"Detected {len(merged_segments)} speech segments, total duration: {total_speech:.2f}s")

        return merged_segments

    def estimate_sentence_timings(self):
        """Estimate timestamps by mapping sentences to speech segments (mimics Whisper's encoding and timestamping)."""
        if not self.sentences:
            print("No sentences to process.")
            return []

        num_sentences = len(self.sentences)
        if num_sentences == 0:
            print("No sentences found.")
            return []

        # Chunk audio into speech segments
        speech_segments = self.chunk_audio()

        # Calculate total speech duration
        speech_duration = sum(end - start for start, end in speech_segments)
        speech_duration = min(speech_duration, self.audio_duration)

        # Calculate duration per sentence, weighted by segment duration
        total_chars = sum(len(s) for s in self.sentences)
        char_duration = speech_duration / total_chars if total_chars > 0 else 1.0

        timings = []
        current_time = speech_segments[0][0] if speech_segments else 0.0
        total_assigned_time = 0.0
        sentence_idx = 0
        segment_idx = 0

        while sentence_idx < len(self.sentences) and segment_idx < len(speech_segments):
            sentence = self.sentences[sentence_idx]
            sentence_length = len(sentence)
            duration = sentence_length * char_duration
            duration = max(duration, 0.5)  # Minimum duration

            print(f"Sentence {sentence_idx + 1}: '{sentence}' (Length: {sentence_length}) Duration: {duration:.2f}s")

            segment_start, segment_end = speech_segments[segment_idx]
            segment_duration = segment_end - segment_start

            if current_time + duration <= segment_end and total_assigned_time + duration <= self.audio_duration:
                timings.append({
                    'text': sentence,
                    'start_time': current_time,
                    'end_time': current_time + duration
                })
                current_time += duration
                total_assigned_time += duration
                sentence_idx += 1
            else:
                segment_idx += 1
                if segment_idx < len(speech_segments):
                    current_time = speech_segments[segment_idx][0]
                else:
                    break

        # Handle remaining sentences
        remaining_sentences = self.sentences[sentence_idx:]
        if remaining_sentences:
            remaining_time = self.audio_duration - total_assigned_time
            remaining_chars = sum(len(s) for s in remaining_sentences)
            if remaining_time > 0 and remaining_chars > 0:
                char_duration = remaining_time / remaining_chars
                for sentence in remaining_sentences:
                    duration = len(sentence) * char_duration
                    duration = max(duration, 0.5)
                    if total_assigned_time + duration > self.audio_duration:
                        duration = self.audio_duration - total_assigned_time
                        if duration <= 0:
                            break
                    timings.append({
                        'text': sentence,
                        'start_time': current_time,
                        'end_time': current_time + duration
                    })
                    current_time += duration
                    total_assigned_time += duration

        # Debug: Print timings
        for i, timing in enumerate(timings):
            if (i + 1) % 10 == 0 or i == len(timings) - 1:
                print(f"Sentence {i+1}: {self.format_time(timing['start_time'])} --> {self.format_time(timing['end_time'])}, Text: {timing['text'][:20]}...")

        print(f"Total assigned duration in SRT: {total_assigned_time:.2f}s (Audio duration: {self.audio_duration:.2f}s)")
        return timings

    def format_time(self, seconds):
        """Convert seconds to SRT time format (HH:MM:SS,mmm)."""
        hours = int(seconds // 3600)
        minutes = int((seconds % 3600) // 60)
        secs = int(seconds % 60)
        millis = int((seconds % 1) * 1000)
        return f"{hours:02d}:{minutes:02d}:{secs:02d},{millis:03d}"

    def generate_srt(self, output_srt_path):
        """Generate SRT file with UTF-8 BOM from timings."""
        timings = self.estimate_sentence_timings()
        with open(output_srt_path, 'w', encoding='utf-8-sig') as file:
            for i, timing in enumerate(timings, 1):
                start_time = self.format_time(timing['start_time'])
                end_time = self.format_time(timing['end_time'])
                text = timing['text'].strip()
                file.write(f"{i}\n")
                file.write(f"{start_time} --> {end_time}\n")
                file.write(f"{text}\n\n")
        print(f"SRT file generated with UTF-8 BOM: {output_srt_path}")



# Example usage
if __name__ == "__main__":
    folder = r"I:\Audio Books\赵旭\一个69届初中生的文革十年"
    file = r"一个69届初中生的文革十年-第一章　风萧萧大年三十与父母一同插队落户积石山"
    mp3 = rf"{folder}/mp3/{file}.mp3"
    wav = rf"{folder}/wav/{file}.wav"
    txt = rf"{folder}/txt/{file}.txt"
    srt = rf"{folder}/mp4/{file}.srt"

    audio = Audio(mp3, txt)
    audio.generate_srt(srt)