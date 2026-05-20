import edge_tts
import tempfile
from services.logger import Logger
from models.file import File

class TtsService:
    def __init__(self):
        self.logger = Logger(self.__class__.__name__)
        self.default_voice = "en-US-AriaNeural"
        self.output_dir = tempfile.gettempdir()
        self.supported_formats = {
            "audio-16khz-128kbitrate-mono-mp3": "mp3",
            "audio-24khz-160kbitrate-mono-mp3": "mp3",
            "riff-24khz-16bit-mono-pcm": "wav",
        }
        self.logger.info("TtsService initialized.")

    async def generate_speech(self, text: str, voice: str, output_file: str) -> str:
        """Generate speech from text using Edge TTS"""
        try:
            # voice = 'zh-CN-YunjianNeural'
            self.logger.info(f"Generating speech with voice: {voice} for text: {text[:20]}... ")

            communicate = edge_tts.Communicate(text, voice, volume='+15%')
            await communicate.save(output_file)

            self.logger.info(f"Speech saved to {output_file}")

            return output_file
        except Exception as e:
            self.logger.error(f"Error generating speech: {str(e)}")
            raise

    def get_output_file_path(self, prefix: str = "tts_", suffix: str = ".mp3") -> str:
        """Generate a unique temporary file path."""
        return File.get_temp_file(prefix=prefix, suffix=suffix)

    def list_voices(self) -> list:
        """Get list of available voices"""
        self.logger.info("Listing available voices.")
        return edge_tts.list_voices()
