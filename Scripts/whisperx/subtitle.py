import whisperx
import torchaudio
import os
import torch

def format_timestamp(seconds: float) -> str:
    hours = int(seconds // 3600)
    minutes = int((seconds % 3600) // 60)
    secs = int(seconds % 60)
    milliseconds = int((seconds - int(seconds)) * 1000)
    return f"{hours:02}:{minutes:02}:{secs:02},{milliseconds:03}"

def write_srt(segments, output_path="output.srt"):
    with open(output_path, "w", encoding="utf-8") as f:
        for i, segment in enumerate(segments, 1):
            start = format_timestamp(segment["start"])
            end = format_timestamp(segment["end"])
            text = segment["text"].strip()
            f.write(f"{i}\n{start} --> {end}\n{text}\n\n")
    print(f"SRT file saved to {output_path}")

def main(audio_path, transcript_path, output_srt="output.srt", language="zh"):
    device = "cuda" if torch.cuda.is_available() else "cpu"
    print(f"Using device: {device}")

    # Load audio duration
    metadata = torchaudio.info(audio_path)
    audio_duration = metadata.num_frames / metadata.sample_rate

    # Read transcript
    with open(transcript_path, "r", encoding="utf-8") as f:
        transcript = f.read().strip()

    # Load Whisper model
    print("Loading Whisper model...")
    model = whisperx.load_model("turbo", device=device)

    # Create a dummy segment with full audio range and transcript
    segments = [{
        "start": 0.0,
        "end": audio_duration,
        "text": transcript
    }]

    # Load alignment model
    print("Loading alignment model...")
    align_model, metadata = whisperx.load_align_model(language_code=language, device=device)

    print("Aligning...")
    result_aligned = whisperx.align(segments, align_model, metadata, audio_path, device)

    # Write to SRT
    print(f"Writing to SRT {output_srt}...")
    write_srt(result_aligned["segments"], output_path=output_srt)

if __name__ == "__main__":\
    # python subtitle.py "I:\Audio Books\余华\活着\mp3\活着-第五章.mp3" "I:\Audio Books\余华\活着\txt\活着-第五章.txt"
    import sys
    if len(sys.argv) != 3:
        print("Usage: python align_to_srt.py path/to/audio.wav path/to/transcript.txt")
    else:
        main(sys.argv[1], sys.argv[2])
