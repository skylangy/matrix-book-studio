
from duration_generator import DurationSubtitleGenerator
from wavform_generator import WavFormSubtitleGenerator
from whisper_generato import WhisperSubtitleGenerator
import os
from datetime import datetime

class BookFolderScanner:
    def __init__(self, base_folder) -> None:
        self.base_folder = base_folder
        self.generator = DurationSubtitleGenerator()

    def scan_library(self):
        for author_folder in os.listdir(self.base_folder):
            author_folder_path = rf"{self.base_folder}\{author_folder}"
            if os.path.isdir(author_folder_path):
                self.scan_author_folder(author_folder_path)

    def scan_author_folder(self, folder: str = None):
        if folder is None:
            folder = self.base_folder

        for book_folder in os.listdir(folder):
            book_folder_path = rf"{folder}\{book_folder}"
            if os.path.isdir(book_folder_path) and self.is_book_folder(book_folder_path):
                self.log(f"Scanning book {book_folder_path}")
                self.scan_book_folder(book_folder_path)

    def scan_book_folder(self, folder: str = None):
        if folder is None:
            folder = self.base_folder

        if not self.is_book_folder(folder):
            self.log(f"{folder} is not a valid book folder")
            return

        try:
            txt_files = self.get_file_names_without_extension(rf"{folder}\txt")


            for txt_file in txt_files:
                mp3_path = rf"{folder}\mp3\{txt_file}.mp3"
                txt_path = rf"{folder}\txt\{txt_file}.txt"
                mp4_path = rf"{folder}\mp4\{txt_file}.srt"

                self.log(f"Processing {txt_file}")

                self.generator.generate(mp3_path, txt_path, mp4_path)

                self.log(f"Generated subtitle for {txt_file}")
        except Exception as e:
            self.log(f"Error processing {folder} failed: {e}")

    def is_book_folder(self, folder):
        txt_folder = rf"{folder}\txt"
        mp3_folder = rf"{folder}\mp3"
        mp4_folder = rf"{folder}\mp4"

        return os.path.exists(txt_folder) and os.path.exists(mp3_folder) and os.path.exists(mp4_folder)

    def get_file_names_without_extension(self, folder_path):
        file_names = []
        for file in os.listdir(folder_path):
            if os.path.isfile(os.path.join(folder_path, file)):
                file_name, _ = os.path.splitext(file)
                file_names.append(file_name)
        return file_names

    def log(self, message):
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        print(f'[{timestamp}] [{self.__class__.__name__}] {message}')
