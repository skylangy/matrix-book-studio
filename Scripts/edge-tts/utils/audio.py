from pydub import AudioSegment

class AudioConverter:
    def __init__(self, logger):
        self.logger = logger

    def convert_to_wav(self, file_path: str) -> str:
        """Convert an audio file to WAV format."""
        self.logger.info(f"Converting file '{file_path}' to WAV format.")

        audio = AudioSegment.from_file(file_path)
        wav_file_path = file_path.replace(".mp3", ".wav")
        audio.export(wav_file_path, format="wav")

        self.logger.info(f"Converted' {file_path}' to '{wav_file_path}'")
        return wav_file_path

    def convert_to_mp3(self, file_path: str) -> str:
        """Convert an audio file to MP3 format."""
        self.logger.info(f"Converting file {file_path} to MP3 format.")
        audio = AudioSegment.from_file(file_path)
        mp3_file_path = file_path.replace(".wav", ".mp3")
        audio.export(mp3_file_path, format="mp3")
        self.logger.info(f"Converted {file_path} to {mp3_file_path}")
        return mp3_file_path