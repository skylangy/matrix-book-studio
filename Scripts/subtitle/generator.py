from folder_scanner import BookFolderScanner
from wavform_generator import WavFormSubtitleGenerator
from whisper_generato import WhisperSubtitleGenerator
from duration_generator import DurationSubtitleGenerator


def scan_library():
    base_path = r"G:\Audio Books"
    scanner = BookFolderScanner(base_path)
    scanner.scan_library()

def scan_author_folder():
    base_path = r"G:\Audio Books\高罗佩"
    scanner = BookFolderScanner(base_path)
    scanner.scan_author_folder()

def scan_book_folder():
    base_path = r"G:\Audio Books\沈星星\边水往事"
    scanner = BookFolderScanner(base_path)
    scanner.scan_book_folder()

def scan_single_book():
    base_path =  r"G:\Audio Books\王友琴\文革受难者"
    name = "文革受难者-第三章"
    mp3_path = rf"{base_path}\mp3\{name}.mp3"
    script_path = rf"{base_path}\txt\{name}.txt"
    output_path = rf"{base_path}\mp4\{name}.srt"
    generator = DurationSubtitleGenerator()
    generator.generate(mp3_path, script_path, output_path)

def scan_wavform():
    base_path = r"G:\Audio Books\高罗佩\大唐狄公案-紫云寺"
    name = "大唐狄公案-紫云寺-第一章"
    mp3_path = rf"{base_path}\mp3\{name}.mp3"
    script_path = rf"{base_path}\txt\{name}.txt"
    output_path = rf"{base_path}\mp4\{name}.srt"
    generator = WhisperSubtitleGenerator()
    generator.generate(mp3_path, script_path, output_path)


if __name__ == "__main__":
    # scan_library()
    # scan_author_folder()
    # scan_book_folder()
    # scan_wavform()
    scan_single_book()
