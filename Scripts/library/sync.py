import os
import subprocess
from typing import List
import time

# Constants
MP3_FOLDER = "mp3"
IMAGES_FOLDER = "images"
BOOK_METADATA_FILE = "book_metadata.json"
AUTHOR_METADATA_FILE = "author_metadata.json"


class BaseEntity:
    def __init__(self, name, path):
        self.name = name
        self.path = path

    def has_file(self, filepath):
        return os.path.exists(filepath)


class Book(BaseEntity):
    def __init__(self, name, path):
        super().__init__(name, path)
        self.mp3_folder = os.path.join(path, MP3_FOLDER)
        self.images_folder = os.path.join(path, IMAGES_FOLDER)
        self.metadata_file = os.path.join(path, BOOK_METADATA_FILE)

    def has_mp3(self):
        return self.has_file(self.mp3_folder)

    def has_images(self):
        return self.has_file(self.images_folder)

    def has_metadata(self):
        return self.has_file(self.metadata_file)


class Artist(BaseEntity):
    def __init__(self, name, path):
        super().__init__(name, path)
        self.images_folder = os.path.join(path, IMAGES_FOLDER)
        self.metadata_file = os.path.join(path, AUTHOR_METADATA_FILE)
        self.books = []

    def has_images(self):
        return self.has_file(self.images_folder)

    def has_metadata(self):
        return self.has_file(self.metadata_file)


class FileTransfer:
    def transfer_file(self, source, destination):
        raise NotImplementedError("This method should be implemented by subclasses.")


class WslFileTransfer(FileTransfer):
    def __init__(self, username, hostname, remote_path):
        self.username = username
        self.hostname = hostname
        self.remote_path = remote_path
        self.destination_root = f"{self.username}@{self.hostname}:{self.remote_path}"

    def transfer_file(self, source, destination):
        source_path = self.to_wsl_path(source)
        full_destination = f"{self.destination_root}/{destination}"
        for attempt in range(3):
            try:
                command = ["wsl", "rsync", "-av", "--progress", source_path, full_destination]
                subprocess.run(command, check=True)
                print(f"Successfully transferred {source_path} -> {full_destination}")
                return
            except subprocess.CalledProcessError as e:
                print(f"Error transferring {source}: {e}")
        print(f"Failed to transfer {source} after 3 attempts.")

    @staticmethod
    def to_wsl_path(path):
        return subprocess.check_output(["wsl", "wslpath", "-u", path]).decode().strip()


class LibraryManager:
    def __init__(self, source: str, destination: str, transfer: FileTransfer):
        self.source = source
        self.destination = destination
        self.transfer = transfer

    def sync(self):
        artists = self.scan_library()  # Scan the library
        self.copy_artist(artists)      # Copy the artist data

    def scan_library(self) -> List[Artist]:
        artists = []
        for entry in os.scandir(self.source):
            if entry.is_dir():
                artist = Artist(entry.name, entry.path)
                for sub_entry in os.scandir(entry.path):
                    if sub_entry.is_dir() and sub_entry.name != IMAGES_FOLDER:
                        book = Book(sub_entry.name, sub_entry.path)
                        artist.books.append(book)
                artists.append(artist)
        return artists

    def copy_artist(self, artists: List[Artist]):
        for artist in artists:
            print(f'Copying artist: "{artist.name}"')
            artist_destination = os.path.join(self.destination, artist.name)

            self._transfer_entity(artist, artist.name)
            self.copy_books(artist.books, artist.name)

    def copy_books(self, books: List[Book], artist_name: str):
        for book in books:
            print(f'\tCopying book: {book.name}')
            book_destination = os.path.join(artist_name, book.name)
            self._transfer_entity(book, book_destination)

    def _transfer_entity(self, entity, destination):
        if entity.has_metadata():
            self.transfer.transfer_file(entity.metadata_file, destination)
        if entity.has_images():
            self.transfer.transfer_file(entity.images_folder, destination)
        if isinstance(entity, Book) and entity.has_mp3():
            self.transfer.transfer_file(entity.mp3_folder, destination)


def main():
    source = r'G:\Audio Books'
    destination = r'192.168.7.174:/mnt/data/books'

    file_transfer = WslFileTransfer('andy', '192.168.7.174', '/mnt/data/books')
    manager = LibraryManager(source, destination, file_transfer)

    start_time = time.time()
    manager.sync()

    elapsed_time = time.time() - start_time
    print(f"Sync completed in {elapsed_time:.2f} seconds")

if __name__ == "__main__":
    main()
