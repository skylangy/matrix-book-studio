import librosa
import numpy as np
import re
from typing import List, Tuple
import string

class TextSegment:
    def __init__(self, content: str, estimated_duration: float = 0.0):
        """
        Initialize a TextSegment object.

        Args:
            content (str): The text content of the segment.
            estimated_duration (float): Estimated duration of the segment in seconds.
        """
        self.content = content.strip()
        self.estimated_duration = estimated_duration
        self.word_count = len(self.content)
        self.pronounce_word_count = self.count_pronounce_word()  # Default to word count

    def count_pronounce_word(self) -> int:
        """
        Count pronounced words, excluding punctuation.
        """
        # Define Chinese and English punctuation to exclude
        chinese_punctuation = ',.!?""<>()。！？；，、：“”《》()'
        all_punctuation = string.punctuation + chinese_punctuation
        # Remove punctuation
        clean_text = re.sub(f'[{re.escape(all_punctuation)}]', '', self.content)
        # Count characters (each is a pronounced unit)
        return len(clean_text)

    def __repr__(self):
        return (f"TextSegment(content='{self.content[:30]}...', "
                f"estimated_duration={self.estimated_duration:.2f}s)"
                if len(self.content) > 30 else
                f"TextSegment(content='{self.content}', "
                f"estimated_duration={self.estimated_duration:.2f}s)")

class AudioSegment:
    def __init__(self, start_time: float, end_time: float, audio_data: np.ndarray,
                 sample_rate: int, preceding_silence: float = 0.0):
        """
        Initialize a Segment object.

        Args:
            start_time (float): Start time of the segment in seconds.
            end_time (float): End time of the segment in seconds.
            audio_data (np.ndarray): Audio samples for the segment.
            sample_rate (int): Sample rate of the audio.
            preceding_silence (float): Silence duration before this segment, in seconds.
        """
        self.start_time = start_time
        self.end_time = end_time
        self.audio_data = audio_data
        self.sample_rate = sample_rate
        self.preceding_silence = preceding_silence
        self.duration = end_time - start_time
        self.active_duration = (end_time - start_time) - preceding_silence

    def __repr__(self):
        return (f"AudioSegment(start_time={self.start_time:.2f}s, "
                f"end_time={self.end_time:.2f}s, "
                f"active_duration={self.active_duration:.2f}s, "
                f"preceding_silence={self.preceding_silence:.2f}s, "
                f"sample_rate={self.sample_rate}Hz)")

class Text:
    def __init__(self, file: str):
        self.path = file
        with open(file, 'r', encoding='utf-8') as f:
            self.text = f.read()

        self.segments: List[TextSegment] = self.split_into_segments()
        self.word_count = len(self.text)
        self.pronounce_word_count = sum(seg.pronounce_word_count for seg in self.segments)
        self.avg_word_duration = 0.0

    def split_into_segments(self, punctuation: str = r"[。！？!?；;.,，]", min_length: int = 15, max_length = 50) -> List[TextSegment]:
        """
        Splits the text into segments using the specified punctuation.
        """
        if min_length > max_length:
            raise ValueError("min_length cannot be greater than max_length")
        if min_length < 1:
            raise ValueError("min_length must be at least 1")

        raw_segments = re.split(f'({punctuation})', self.text)
        self.segments = []
        current_segment = ""

        # Step 1: Create initial segments based on punctuation
        initial_segments = []
        for part in raw_segments:
            if not part.strip():
                continue
            current_segment += part
            if re.match(punctuation, part):
                initial_segments.append(current_segment.strip())
                current_segment = ""
        if current_segment.strip():
            initial_segments.append(current_segment.strip())

        # Step 2: Merge or split segments to meet min_length and max_length
        current_text = ""
        current_count = 0
        for segment in initial_segments:
            temp_seg = TextSegment(segment)
            temp_count = temp_seg.word_count

            if current_count + temp_count < min_length:
                # Merge with current text if below min_length
                current_text += " " + segment if current_text else segment
                current_count += temp_count
            else:
                # Process current accumulated text
                if current_text:
                    current_text += " " + segment if current_count > 0 else segment
                    current_count += temp_count
                else:
                    current_text = segment
                    current_count = temp_count

                # Split if above max_length or finalize if within bounds
                while current_count > max_length:
                    # Split at max_length, preferring spaces or punctuation
                    clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', current_text)
                    split_point = min(max_length, len(clean_text))
                    # Try to find a natural boundary (space or punctuation) near split_point
                    split_index = len(current_text)
                    for i in range(min(len(current_text), split_point - 1), -1, -1):
                        if current_text[i] in ' 。！？；，、：“”《》()':
                            split_index = i + 1
                            break
                    new_content = current_text[:split_index]
                    remaining_content = current_text[split_index:] or " "
                    new_seg = TextSegment(new_content)
                    self.segments.append(new_seg)
                    print(f"Split - Segment: Chars: {new_seg.word_count}, '{new_content}'")
                    current_text = remaining_content
                    current_count = TextSegment(current_text).word_count

                if min_length <= current_count <= max_length:
                    self.segments.append(TextSegment(current_text))
                    print(f"Added new segment:  Chars: {current_count}, '{current_text}'")
                    current_text = ""
                    current_count = 0
                elif current_count < min_length:
                    # Carry over to merge with next segment
                    continue
                else:
                    # Further splitting needed (rare case)
                    clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', current_text)
                    split_point = max_length
                    new_content = clean_text[:split_point]
                    remaining_content = clean_text[split_point:] or " "
                    self.segments.append(TextSegment(new_content))
                    print(f"Split - Segment: '{new_content}', Chars: {TextSegment(new_content).word_count}")
                    current_text = remaining_content
                    current_count = TextSegment(current_text).word_count

        # Handle any remaining text
        if current_text and current_count > 0:
            if current_count < min_length and self.segments:
                # Merge with last segment if below min_length
                last_seg = self.segments[-1]
                combined_text = last_seg.content + " " + current_text
                combined_count = TextSegment(combined_text).word_count
                if combined_count <= max_length:
                    self.segments[-1] = TextSegment(combined_text)
                    print(f"Merged with last - Segment: '{combined_text[:30]}...', Chars: {combined_count}")
                else:
                    # Split combined text if exceeds max_length
                    clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', combined_text)
                    split_point = max_length
                    new_content = clean_text[:split_point]
                    remaining_content = clean_text[split_point:] or " "
                    self.segments[-1] = TextSegment(new_content)
                    print(f"Split last - Segment: '{new_content[:30]}...', Chars: {TextSegment(new_content).word_count}")
                    if TextSegment(remaining_content).word_count > 0:
                        self.segments.append(TextSegment(remaining_content))
                        print(f"Added remaining - Segment: '{remaining_content[:30]}...', Chars: {TextSegment(remaining_content).word_count}")
            else:
                # Add as is if within bounds or split if too long
                while current_count > max_length:
                    clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', current_text)
                    split_point = max_length
                    new_content = clean_text[:split_point]
                    remaining_content = clean_text[split_point:] or " "
                    self.segments.append(TextSegment(new_content))
                    print(f"Split final - Segment: '{new_content[:30]}...', Chars: {TextSegment(new_content).word_count}")
                    current_text = remaining_content
                    current_count = TextSegment(current_text).word_count
                if current_count >= min_length:
                    self.segments.append(TextSegment(current_text))
                    print(f"Added final - Segment: '{current_text[:30]}...', Chars: {current_count}")

        return self.segments

    def estimate_durations(self, total_active_duration:float):
        """
        Estimate duration for each text segment based on average word duration.
        Uses a simple proportional alignment to avoid recursive calls.
        Excludes silences from audio and punctuation from text.
        """
        total_word_count = sum(ts.count_pronounce_word() for ts in self.segments)
        self.avg_word_duration = total_active_duration / total_word_count if total_word_count > 0 else 0.1

        # Assign estimated duration to each text segment
        for text_seg in self.segments:
            text_seg.estimated_duration = text_seg.count_pronounce_word() * self.avg_word_duration

class Audio:
    def __init__(self, path: str):
        self.path = path
        self.data, self.sample_rate = librosa.load(path, sr=None)
        self.duration = librosa.get_duration(y=self.data, sr=self.sample_rate)
        self.segments: List[AudioSegment] = self.detect_segments()
        self.active_duration = self.calculate_total_active_duration()

    def detect_segments(self, top_db: float = 30, min_silence: float = 0.05) -> List[AudioSegment]:
        """
        Detects non-silent segments and merges segments if the silence before them is too short.
        """
        intervals = librosa.effects.split(self.data, top_db=top_db)
        merged_segments = []

        previous_end_time = 0.0
        for i, (start_frame, end_frame) in enumerate(intervals):
            start_time = start_frame / self.sample_rate
            end_time = end_frame / self.sample_rate
            duration = end_time - start_time

            # Skip segments with zero or negative duration
            if duration <= 0:
                print(f"Skipping invalid segment {i}: start={start_time:.2f}s, end={end_time:.2f}s, duration={duration:.2f}s")
                continue

            audio_data = self.data[start_frame:end_frame]
            silence = max(0.0, start_time - previous_end_time)

            if merged_segments and silence < min_silence:
                # Merge with previous segment
                prev = merged_segments[-1]
                combined_audio = np.concatenate([prev.audio_data, audio_data])
                new_end_time = end_time
                new_duration = new_end_time - prev.start_time

                # Verify merged segment has positive duration
                if new_duration > 0:
                    merged_segments[-1] = AudioSegment(
                        start_time=prev.start_time,
                        end_time=new_end_time,
                        audio_data=combined_audio,
                        sample_rate=self.sample_rate,
                        preceding_silence=prev.preceding_silence
                    )
                else:
                    print(f"Skipping merged segment: start={prev.start_time:.2f}s, end={new_end_time:.2f}s, duration={new_duration:.2f}s")
                    merged_segments.pop()  # Remove invalid previous segment
            else:
                segment = AudioSegment(
                    start_time=start_time,
                    end_time=end_time,
                    audio_data=audio_data,
                    sample_rate=self.sample_rate,
                    preceding_silence=silence
                )
                merged_segments.append(segment)

            previous_end_time = end_time

        # Filter out any remaining segments with zero or negative duration
        final_segments = [seg for seg in merged_segments if seg.end_time - seg.start_time > 0]
        if len(final_segments) < len(merged_segments):
            print(f"Filtered out {len(merged_segments) - len(final_segments)} segments with invalid duration")

        return final_segments

    def calculate_total_active_duration(self) -> float:
        """
        Calculate the total active duration of all audio segments, excluding silences.
        """
        return sum(segment.active_duration for segment in self.segments)

class AlignResult:
    def __init__(self, start_time: float, end_time: float, text: str):
        self.start_time = start_time
        self.end_time = end_time
        self.text = text

    def __repr__(self):
        return f"{self.start_time:.2f}s -> {self.end_time:.2f}s: {self.text[:30]}..."

class AudioTextAligner:
    def __init__(self, audio_path: str, text_path: str):
        self.audio = Audio(audio_path)
        self.text = Text(text_path)
        self.text.estimate_durations(self.audio.active_duration)
        self.align_results: List[AlignResult] = []

    def align(self) -> List[AlignResult]:
        """
        Aligns the text segments to the audio segments.
        """
        text_segments = self.text.segments
        audio_segments = self.audio.segments
        self.align_results = self.align_audio_to_text(audio_segments, text_segments)
        # self.align_text_to_audio(audio_segments, text_segments)

        return self.align_results

    def to_srt(self, file:str):
        """
        Write the aligned segments to an SRT file.
        """
        with open(file, 'w', encoding='utf-8-sig') as f:
            for i, result in enumerate(self.align_results, start=1):
                start = self.format_timestamp(result.start_time)
                end = self.format_timestamp(result.end_time)
                f.write(f"{i}\n{start} --> {end}\n{result.text}\n\n")

    def format_timestamp(self, seconds: float) -> str:
        """
        Format seconds into SRT timestamp format.
        """
        hours = int(seconds // 3600)
        minutes = int((seconds % 3600) // 60)
        secs = int(seconds % 60)
        milliseconds = int((seconds - int(seconds)) * 1000)
        return f"{hours:02}:{minutes:02}:{secs:02},{milliseconds:03}"

    def align_text_to_audio(self, audio_segments: List[AudioSegment], text_segments: List[TextSegment], min_length: int = 20, max_length: int = 50) -> List[AlignResult]:
        """
        Aligns text segments to audio segments based on duration similarity, respecting min_length and max_length constraints.
        Each result's start_time includes the preceding silence.

        Args:
            audio_segments (List[AudioSegment]): List of audio segments.
            text_segments (List[TextSegment]): List of text segments.
            min_length (int): Minimum character count per aligned segment (excluding punctuation).
            max_length (int): Maximum character count per aligned segment (excluding punctuation).

        Returns:
            List[AlignResult]: List of aligned segments with adjusted start times.
        """
        if not text_segments:
            raise ValueError("Text segments are empty. Provide valid text segments.")
        if not audio_segments:
            raise ValueError("Audio segments are empty. Provide valid audio segments.")
        if min_length > max_length:
            raise ValueError("min_length cannot be greater than max_length")

        # Cache durations and word counts
        self.text.estimate_durations(self.audio.active_duration)
        avg_word_duration = self.text.avg_word_duration
        text_segment_count = len(text_segments)
        total_word_count = sum(seg.word_count for seg in text_segments)

        # Debugging information
        print("=" * 60)
        print(f"Total word count: {total_word_count}")
        print(f"Average word duration: {avg_word_duration:.3f}s")
        print(f"Total audio segments: {len(audio_segments)}")
        print(f"Total audio duration: {self.audio.duration:.2f}s")
        print("=" * 60)

        align_results = []
        text_index = 0

        for audio_seg in audio_segments:
            audio_duration = audio_seg.active_duration
            if audio_duration <= 0:
                print(f"Skipping empty audio segment: {audio_seg}")
                continue

            # Include preceding silence in start_time
            start_time = max(0.0, audio_seg.start_time - audio_seg.preceding_silence)
            end_time = audio_seg.end_time

            print(f"Audio Segment: {start_time:.2f}s - {end_time:.2f}s, Active: {audio_duration:.2f}s, Silence: {audio_seg.preceding_silence:.2f}s")

            # Collect text segments
            current_text_segs = []
            current_text_duration = 0.0
            current_char_count = 0

            while text_index < text_segment_count:
                current_text_segment = text_segments[text_index]
                if not current_text_segs:
                    print(f"\t Starting new text segment: {current_text_segment.content}")
                    current_text_segs.append(current_text_segment)
                    current_text_duration = current_text_segment.estimated_duration
                    current_char_count = current_text_segment.word_count
                    text_index += 1
                else:
                    next_seg = current_text_segment
                    temp_duration = current_text_duration + next_seg.estimated_duration
                    temp_char_count = current_char_count + next_seg.word_count
                    current_diff = abs(current_text_duration - audio_duration) / audio_duration if audio_duration > 0 else float('inf')
                    temp_diff = abs(temp_duration - audio_duration) / audio_duration if audio_duration > 0 else float('inf')

                    # Merge if duration improves, within threshold, or needed for min_length, but not exceeding max_length
                    if (temp_diff < current_diff and temp_diff < 0.2) or (temp_char_count <= min_length and temp_char_count <= max_length):
                        current_text_segs.append(next_seg)
                        current_text_duration = temp_duration
                        current_char_count = temp_char_count
                        text_index += 1
                    else:
                        break

            # Check if alignment is valid
            duration_diff = abs(current_text_duration - audio_duration) / audio_duration if audio_duration > 0 else float('inf')
            if duration_diff < 0.2 and min_length <= current_char_count <= max_length:
                combined_text = " ".join(seg.content for seg in current_text_segs)
                align_results.append(AlignResult(start_time, end_time, combined_text))
                print(f"Match - Audio: {audio_duration:.2f}s, Text: {current_text_duration:.2f}s, "
                      f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")
            else:
                # Handle mismatches
                if current_text_duration > audio_duration * 1.2 or current_char_count > max_length:
                    # Split text
                    chars_needed = min(int(audio_duration / avg_word_duration), max_length)
                    chars_needed = max(chars_needed, min_length)
                    current_chars = current_char_count

                    if chars_needed < current_chars:
                        accumulated_chars = 0
                        split_segs = []
                        for seg in current_text_segs:
                            seg_chars = seg.word_count
                            if accumulated_chars + seg_chars <= chars_needed:
                                split_segs.append(seg)
                                accumulated_chars += seg_chars
                            else:
                                clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', seg.content)
                                split_point = min(chars_needed - accumulated_chars, len(clean_text))
                                new_content = clean_text[:split_point]
                                remaining_content = clean_text[split_point:] or " "
                                split_segs.append(TextSegment(new_content, len(new_content) * avg_word_duration))
                                # Insert remaining content back into text_segments
                                text_segments.insert(text_index, TextSegment(remaining_content, len(remaining_content) * avg_word_duration))
                                text_segment_count += 1
                                text_index += 1
                                break
                        else:
                            # All segments used, no split needed
                            pass

                        combined_text = " ".join(seg.content for seg in split_segs)
                        new_char_count = sum(seg.word_count for seg in split_segs)
                        new_duration = sum(seg.estimated_duration for seg in split_segs)
                        align_results.append(AlignResult(start_time, end_time, combined_text))
                        print(f"Split - Audio: {audio_duration:.2f}s, Text: {current_text_duration:.2f}s, "
                              f"New Text: {new_duration:.2f}s, Chars: {new_char_count}")
                    else:
                        # No split needed, but duration mismatch
                        print(f"Skip - Audio: {audio_duration:.2f}s, Text: {current_text_duration:.2f}s, "
                              f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")
                else:
                    # Merge more if too short
                    while (text_index < text_segment_count and
                           (current_text_duration < audio_duration * 0.8 or current_char_count < min_length) and
                           current_char_count < max_length):
                        next_seg = current_text_segment
                        current_text_segs.append(next_seg)
                        current_text_duration += next_seg.estimated_duration
                        current_char_count += next_seg.word_count
                        text_index += 1

                    duration_diff = abs(current_text_duration - audio_duration) / audio_duration if audio_duration > 0 else float('inf')
                    if duration_diff < 0.2 and min_length <= current_char_count <= max_length:
                        combined_text = " ".join(seg.content for seg in current_text_segs)
                        align_results.append(AlignResult(start_time, end_time, combined_text))
                        print(f"Merge - Audio: {audio_duration:.2f}s, Text: {current_text_duration:.2f}s, "
                              f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")
                    else:
                        print(f"Skip - Audio: {audio_duration:.2f}s, Text: {current_text_duration:.2f}s, "
                              f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")

        return align_results

    def align_audio_to_text(self, audio_segments: List[AudioSegment], text_segments: List[TextSegment], min_length: int = 20, max_length: int = 50) -> List[AlignResult]:
        """
        Aligns audio segments to text segments by looping text segments first, then finding matching audio segments.
        Each result's start_time includes the preceding silence.

        Args:
            audio_segments (List[AudioSegment]): List of audio segments.
            text_segments (List[TextSegment]): List of text segments.
            min_length (int): Minimum character count per aligned segment (excluding punctuation).
            max_length (int): Maximum character count per aligned segment (excluding punctuation).

        Returns:
            List[AlignResult]: List of aligned segments with adjusted start times.
        """
        if not text_segments:
            raise ValueError("Text segments are empty. Provide valid text segments.")
        if not audio_segments:
            raise ValueError("Audio segments are empty. Provide valid audio segments.")
        if min_length > max_length:
            raise ValueError("min_length cannot be greater than max_length")

        # Cache durations and word counts
        self.text.estimate_durations(self.audio.active_duration)
        avg_word_duration = self.text.avg_word_duration
        text_segment_count = len(text_segments)
        total_word_count = sum(seg.word_count for seg in text_segments)

        # Debugging information
        print("=" * 60)
        print(f"Total word count: {total_word_count}")
        print(f"Average word duration: {avg_word_duration:.3f}s")
        print(f"Total audio segments: {len(audio_segments)}")
        print(f"Total audio duration: {self.audio.duration:.2f}s")
        print("=" * 60)

        align_results = []
        audio_index = 0
        text_index = 0

        while text_index < text_segment_count:
            current_text_segs = [text_segments[text_index]]
            current_text_duration = text_segments[text_index].estimated_duration
            current_char_count = text_segments[text_index].word_count
            text_index += 1

            # Merge text segments if below min_length or duration too short
            while (text_index < text_segment_count and
                   (current_char_count < min_length or current_text_duration < 0.5) and
                   current_char_count < max_length):
                next_seg = text_segments[text_index]
                current_text_segs.append(next_seg)
                current_text_duration += next_seg.estimated_duration
                current_char_count += next_seg.word_count
                text_index += 1
                print(f"Merged text - Duration: {current_text_duration:.2f}s, Chars: {current_char_count}")

            # Find matching audio segments
            current_audio_segs = []
            current_audio_duration = 0.0
            start_time = None
            end_time = None

            while audio_index < len(audio_segments) and current_audio_duration < current_text_duration * 0.8:
                audio_seg = audio_segments[audio_index]
                if audio_seg.active_duration <= 0:
                    print(f"Skipping empty audio segment: {audio_seg}")
                    audio_index += 1
                    continue

                current_audio_segs.append(audio_seg)
                current_audio_duration += audio_seg.active_duration
                if not start_time:
                    start_time = max(0.0, audio_seg.start_time - audio_seg.preceding_silence)
                end_time = audio_seg.end_time
                audio_index += 1

            # Check alignment
            duration_diff = abs(current_text_duration - current_audio_duration) / current_text_duration if current_text_duration > 0 else float('inf')
            if duration_diff < 0.2 and min_length <= current_char_count <= max_length:
                combined_text = " ".join(seg.content for seg in current_text_segs)
                align_results.append(AlignResult(start_time, end_time, combined_text))
                print(f"Match - Text: {current_text_duration:.2f}s, Audio: {current_audio_duration:.2f}s, "
                      f"Diff: {duration_diff:.2f}, Chars: {current_char_count}, "
                      f"Start: {start_time:.2f}s, End: {end_time:.2f}s")
            else:
                # Handle mismatches
                if current_char_count > max_length or current_text_duration > current_audio_duration * 1.2:
                    # Split text
                    chars_needed = min(int(current_audio_duration / avg_word_duration), max_length)
                    chars_needed = max(chars_needed, min_length)
                    if chars_needed < current_char_count:
                        accumulated_chars = 0
                        split_segs = []
                        for seg in current_text_segs:
                            seg_chars = seg.word_count
                            if accumulated_chars + seg_chars <= chars_needed:
                                split_segs.append(seg)
                                accumulated_chars += seg_chars
                            else:
                                clean_text = re.sub(f'[{re.escape(string.punctuation + "。！？；，、：“”《》()")}]', '', seg.content)
                                split_point = min(chars_needed - accumulated_chars, len(clean_text))
                                new_content = clean_text[:split_point]
                                remaining_content = clean_text[split_point:] or " "
                                split_segs.append(TextSegment(new_content, len(new_content) * avg_word_duration))
                                # Insert remaining content back
                                text_segments.insert(text_index, TextSegment(remaining_content, len(remaining_content) * avg_word_duration))
                                text_segment_count += 1
                                text_index -= len(current_text_segs) - len(split_segs) + 1
                                break
                        else:
                            pass

                        combined_text = " ".join(seg.content for seg in split_segs)
                        new_char_count = sum(seg.word_count for seg in split_segs)
                        new_duration = sum(seg.estimated_duration for seg in split_segs)
                        align_results.append(AlignResult(start_time, end_time, combined_text))
                        print(f"Split - Text: {current_text_duration:.2f}s, Audio: {current_audio_duration:.2f}s, "
                              f"New Text: {new_duration:.2f}s, Chars: {new_char_count}, "
                              f"Start: {start_time:.2f}s, End: {end_time:.2f}s")
                    else:
                        print(f"Skip - Text: {current_text_duration:.2f}s, Audio: {current_audio_duration:.2f}s, "
                              f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")
                else:
                    print(f"Skip - Text: {current_text_duration:.2f}s, Audio: {current_audio_duration:.2f}s, "
                          f"Diff: {duration_diff:.2f}, Chars: {current_char_count}")

        return align_results


if __name__ == "__main__":
    folder = r"I:\Audio Books\赵旭\一个69届初中生的文革十年"
    file = r"一个69届初中生的文革十年-第四章　我小时候的右派狗崽子记忆"
    mp3 = rf"{folder}/mp3/{file}.mp3"
    wav = rf"{folder}/wav/{file}.wav"
    txt = rf"{folder}/txt/{file}.txt"
    srt = rf"{folder}/mp4/{file}.srt"

    text = Text(txt)
    audio = Audio(mp3)
    aligner = AudioTextAligner(mp3, txt)
    aligned_segments = aligner.align()
    # for result in aligned_segments:
    #     print(result)

    aligner.to_srt(srt)

    # for segement in text.segments:
    #     print(segement)

    print(f"Total audio segments: {len(audio.segments)}")
    print(f"Total text segments: {len(text.segments)}")