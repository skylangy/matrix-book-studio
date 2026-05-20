import os
import tempfile
import logging
import time

logger = logging.getLogger(__name__)

class File:
    def __init__(self, path: str):
        self.path: str = path

    def __repr__(self) -> str:
        return f"File(path={self.path!r})"

    def __str__(self) -> str:
        return self.path

    def __eq__(self, other: object) -> bool:
        return isinstance(other, File) and self.path == other.path

    def delete(self) -> None:
        """Delete the file if it exists."""
        try:
            os.remove(self.path)
            logger.info(f"Deleted file: {self.path}")
        except FileNotFoundError:
            logger.warning(f"File not found during deletion: {self.path}")
        except Exception as e:
            logger.error(f"Error deleting file {self.path}: {e}")

    @staticmethod
    def get_temp_dir() -> str:
        """Return the temp directory path."""
        return tempfile.gettempdir()

    @staticmethod
    def get_temp_file(prefix: str = "tts_", suffix: str = ".mp3") -> str:
        """Generate a unique temp file path without creating the file."""
        filename = f"{prefix}{os.urandom(8).hex()}{suffix}"
        return os.path.join(File.get_temp_dir(), filename)

    @staticmethod
    def try_delete(file_path, retries=3, delay=0.5):
        """Attempt to delete a file, retrying on failure."""
        for _ in range(retries):
            try:
                os.remove(file_path)
                logger.info(f"Deleted file: {file_path}")
                return True
            except Exception as e:
                logger.debug(f"Retry delete failed for {file_path}: {e}")
                time.sleep(delay)
        logger.error(f"Failed to delete file after {retries} attempts: {file_path}")
        return False
