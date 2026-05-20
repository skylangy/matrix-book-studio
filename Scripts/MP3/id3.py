from mutagen.id3 import ID3, TIT2, TPE1, TALB, Encoding, ID3NoHeaderError
from mutagen.mp3 import MP3
import chardet
import os
from argparse import ArgumentParser

class ID3Tag:
    def __init__(self, file):
        try:
            self.file = file
            self.verbose = True

            self.audio = MP3(file, ID3=ID3)
            self.id3 = ID3(self.file)
        except ID3NoHeaderError as e:
            self.log(f"Error loading ID3 tags for '{file}': {e}")
            self.log("Creating new ID3 tags.")
            self.audio = MP3(file)
            self.id3 = ID3() #self.audio.tags

    def update(self):
        if not self.audio.tags:
            self.log(f"No ID3 tags found for '{self.file}'.")
            return

        try:
            self.log('-'* 90)
            self.log(f"Start updating tags for: '{self.file}'")
            title = self.get_metadata('TIT2', TIT2)
            artist = self.get_metadata('TPE1', TPE1)
            album = self.get_metadata('TALB', TALB)

            self.log(f"Update Title to: {title}")
            self.log(f"Update Artist to: {artist}")
            self.log(f"Update Album to: {album}")

            self.id3.add(TIT2(encoding=Encoding.UTF8, text=title))
            self.id3.add(TPE1(encoding=Encoding.UTF8, text=artist))
            self.id3.add(TALB(encoding=Encoding.UTF8, text=album))
            self.id3.save()
            self.log('-'* 90)
        except Exception as e:
            self.log(f"Error updating ID3 tags: {e}")
            self.log('-'* 90)

    def set(self, artist, album, title):
        try:
            self.log('-'* 90)
            self.log(f"Start setting tags for: '{self.file}'")
            self.log(f"Set Title: {title}, artist: {artist}, album: {album}")

            if not self.id3:
                self.log(f"No ID3 tags found for '{self.file}'.")
                self.audio.add_tags()

            self.id3.add(TIT2(encoding=Encoding.UTF8, text=title))
            self.id3.add(TPE1(encoding=Encoding.UTF8, text=artist))
            self.id3.add(TALB(encoding=Encoding.UTF8, text=album))
            self.id3.save(self.file)
            self.log('-'* 90)
        except Exception as e:
            self.log(f"Error setting ID3 tags: {e}")
            self.log('-'* 90)

    def print(self):
        if not self.audio.tags:
            self.log(f"No ID3 tags found for '{self.file}'.")
            return
        self.log('-'* 90)
        self.log(f"ID3 tags found for '{self.file}':")

        self.log(f"Title: {self.get_metadata('TIT2', TIT2)} ")
        self.log(f"Artist: {self.get_metadata('TPE1', TPE1)}")
        self.log(f"Album: {self.get_metadata('TALB', TALB)}")
        self.log('-'* 90)

    def get_metadata(self, name, type):
        if not self.audio.tags:
            self.log(f"No ID3 tags found for '{self.file}'.")
            return None

        tag = self.audio.tags.get(name, None)
        if tag:
            content = tag.text[0] if isinstance(tag, type) else tag
            return self.decode(content)

        return ''

    def decode(self, text):
        try:
            byte_string = bytes(text, 'latin-1')
            encoding = {'encoding':'GB2312'} # chardet.detect(byte_string)

            if self.verbose:
                self.log(f"\tEncoding: {encoding}")

            decoded = byte_string.decode(encoding['encoding'])
            if self.verbose:
                self.log(f"\tDecoded: {text} -> {decoded}")
        except Exception as e:
            self.log(f"Error decoding text: {e}")
            self.log(f'Text to decode: {text}')

            encoding = {'encoding':'latin-1'}
            byte_string = bytes(text, 'latin-1')
            decoded = byte_string.decode(encoding['encoding'])
            if self.verbose:
                self.log(f"\tDecoded: {text} -> {decoded}")
            decoded = text

        return decoded

    def log(self, msg):
        print(f"[ID3Tag] {msg}")

def enumerate_folder(folder: str, file_type='.mp3'):
    mp3_files = []
    for root, dirs, files in os.walk(folder):
        for file in files:
            if file.lower().endswith(file_type):
                mp3_files.append(os.path.join(root, file))
    return mp3_files

def main():
    parser = ArgumentParser(description="Update ID3 tags for MP3 files.")
    parser.add_argument('--func', type=str, default='Set', help='The func to run, "Update", "Set".')
    parser.add_argument('--path', type=str, help='The folder to search for MP3 files.')
    parser.add_argument('--artist', type=str, help='Artist name for the MP3 files.')
    parser.add_argument('--album', type=str, help='Album name for the MP3 files.')
    parser.add_argument('--use_filename_as_title', type=str, help='Use the filename as the title for the MP3 files.')

    args = parser.parse_args()
    root = args.path

    print( args)
    # py .\id3.py --func "Set" --path "C:\Users\Andy\Documents\Music\林忆莲.-.[1993情撼红馆(CD1)].专辑.(MP3)" --artist "林忆莲" --album "1993情撼红馆" --use_filename_as_title "True"
    # py .\id3.py --func "Update" --path "C:\Users\Andy\Documents\Music\Mix"

    mp3_files = enumerate_folder(root)

    if args.func == "Update":
        for file in mp3_files:
            id3 = ID3Tag(file)
            id3.update()
    elif args.func == "Set":
        for file in mp3_files:
            id3 = ID3Tag(file)
            filename = os.path.basename(file)
            file_name_without_extension, _ = os.path.splitext(filename)

            id3.set(args.artist, args.album, file_name_without_extension)

    print(f"Found and processed {len(mp3_files)} MP3 files.")


if __name__ == "__main__":
    main()