from ebooklib import epub, ITEM_DOCUMENT
from bs4 import BeautifulSoup, Comment
from abc import ABC, abstractmethod
from mobi import extract as mobi_extract

import ebooklib
import chardet
import fitz
import mobi
import opencc
import warnings
import os
import subprocess
import argparse
import re
import zipfile
import os
import xml.etree.ElementTree as ET
import shutil
import tempfile

class HtmlTextExtractor():
    classes_to_skip = [
        'noindent-index', 'hide', 'sigil_not_in_toc', 'mbp-toc', 'mbp-toc-title', 'mbp-toc-level1', 'mbp-toc-level2',
        'mbp_pagebreak', 'mbp-pub-pagebreak', 'mbp-pub-pagebreak-2', 'mbp-pub-pagebreak-3', 'mbp-pub-pagebreak-4',
    ]
    tags_to_keep = { "h1", "h2", "h3", "h4", "h5", "h6","p" ,"span"}
    tags_to_skip = { }
    attrs_to_keep = { }
    attrs_to_skip = { "epub:type" }

    def __init__(self, html, file_path):
        self.html = html
        self.file_path = file_path
        self.verbose = False

        self.unicode_map = {
            "\\u3000": " ",  # Ideographic space
            "\\u2003": " ",  # Em space
        }

    def extract_text(self):
        self.html = self.pre_process_html(self.html)

        soup = BeautifulSoup(self.html, "html.parser")

        for comment in soup.find_all(string=lambda text: isinstance(text, Comment)):
            comment.extract()

        # Decompose elements by class and attribute in one loop
        for class_name in self.classes_to_skip:
            for element in soup.select(f'.{class_name}'):
                element.decompose()

        for attr in self.attrs_to_skip:
            for element in soup.find_all(attrs={attr: True}):
                element.decompose()

        # Clean tags: Keep specified tags and filter attributes, remove others
        for tag in soup.find_all(True):
            if tag.name in self.tags_to_keep:
                tag.attrs = {key: value for key, value in tag.attrs.items() if key in self.attrs_to_keep}
            else:
                tag.unwrap()

        cleaned_html = str(soup)

        if self.verbose:
            self.dump_html(cleaned_html)

        plain_text = self.post_process_html(cleaned_html)

        return plain_text

    def dump_html(self, cleaned_html):
        folder, file = os.path.split(self.file_path)
        file_name = os.path.splitext(file)[0]
        destination = os.path.join(folder, f"{file_name}.html")
        raw_html = os.path.join(folder, f"{file_name}-raw.html")

        print(f"Writing cleaned HTML to {destination}")
        print(f"Writing raw HTML to {raw_html}")

        with open(destination, "w", encoding='utf-8') as file:
            file.write(cleaned_html)

        with open(raw_html, "w", encoding='utf-8') as file:
            file.write(self.html)

    def pre_process_html(self, html):
        html = html.replace("&#13;", " ")                   # Replace carriage return with space
        html = re.sub(r"</?span[^>]*>", " ", html)          # Remove <span> tags
        html = re.sub(r"<br\s*/?>", " ", html)              # Replace <br> with space
        html = re.sub(r'(<h[1-6]>)([\s\S]*?)(</h[1-6]>)',
                    lambda m: f"{m.group(1)}{m.group(2).replace(chr(10), '').replace(chr(13), '').strip()}{m.group(3)}",
                    html)                                   # Remove line breaks from headings
        html = re.sub(r'<hr\s*/?>', '', html, flags=re.IGNORECASE)

        return html

    def post_process_html(self, html):
        html = self.remove_tags(html)
        html = self.remove_unicode(html)
        html = self.remove_linebreaks(html)
        html = self.remove_tail_spaces(html)

        return html

    def remove_tags(self, html):
        html = re.sub(r"</\w+>", "\n", html)  # Replace closing tags with newline
        html = re.sub(r"<[^>]+>", " ", html)  # Replace other tags with a space
        return html

    def remove_unicode(self, html):
        for unicode_seq, replacement in self.unicode_map.items():
            html = html.replace(unicode_seq, replacement)
        return html

    def remove_linebreaks(self, html):
        text = re.sub(r"\n{3,}", "\n\n", html)
        text = re.sub(r"^\s+", "", text, flags=re.MULTILINE)
        return text

    def remove_tail_spaces(self, text):
        return re.sub(r"\s+$", "", text, flags=re.MULTILINE)

class TextExtractor(ABC):

    def __init__(self, file_path):
        self.file_path = file_path

    @abstractmethod
    def extract_text(self):
        pass

    def extract_text_to(self, output_path):
        text = self.extract_text()
        text = self.to_simplified_chinese(text)
        if text and len(text) > 0:
            with open(output_path, "w", encoding='utf-8') as file:
                file.write(text)
        else:
            print(f"No text extracted from {self.file_path}")

    def open_with_notepad(self, file_path):
        notepad_plus_plus_path = "C:/Program Files/Notepad++/notepad++.exe"
        subprocess.Popen([notepad_plus_plus_path, file_path])

    def clean_html(self, html):
        extractor = HtmlTextExtractor(html, self.file_path)
        return extractor.extract_text()

    def detect_encoding(self, file_path):
        with open(file_path, "rb") as file:
            rawdata = file.read()
            result = chardet.detect(rawdata)
            return result['encoding']

    def read_text(self, file_path) -> str:
        encodings_to_try = ["utf-8", "ansi"]  # Add other common encodings if needed
        last_exception = None

        print(f"Reading text content from: {file_path}")

        # Try reading with each encoding in the list
        for encoding in encodings_to_try:
            try:
                print(f"Trying to read file with encoding: {encoding}")
                with open(file_path, "r", encoding=encoding) as file:
                    print(f"Successfully read file with encoding: {encoding}")
                    return file.read()
            except UnicodeDecodeError as e:
                print(f"Failed to read with encoding {encoding}: {e}")
                last_exception = e

        # If all encodings fail, try detecting encoding dynamically
        try:
            detected_encoding = self.detect_encoding(file_path)
            print(f"Detected encoding: {detected_encoding} {file_path}")
            with open(file_path, "r", encoding=detected_encoding) as file:
                return file.read()
        except Exception as e:
            print(f"Failed to read file even with detected encoding: {e}")
            last_exception = e

        # If everything fails, re-raise the last exception
        raise last_exception

    def to_simplified_chinese(self, text):
        cc = opencc.OpenCC('t2s')
        return cc.convert(text)

    def is_epub(self, file_path):
        return file_path.endswith(".epub")

class PDFExtractor(TextExtractor):
    def __init__(self, file_path):
        super().__init__(file_path)

    def extract_text(self):
        # Open the PDF file
        pdf_document = fitz.open(self.file_path)
        text = ""
        for page_num in range(pdf_document.page_count):
            page = pdf_document[page_num]
            text += page.get_text("html")

        text = super().clean_html(text)
        return text

class MobiExtractor(TextExtractor):
    def __init__(self, file_path):
        super().__init__(file_path)

    def extract_text(self):
        print(f"Extracting MOBI file: {self.file_path}")
        tempdir, filepath = mobi.extract(self.file_path)
        print(f"Extracted MOBI to {tempdir}, {filepath}")

        if self.is_epub(filepath):
            extractor = EpubExtractor(filepath)
            text = extractor.extract_text()
        else:
            html = super().read_text(filepath)
            text = super().clean_html(html)
        return text

class EpubExtractor(TextExtractor):
    item_type_names = {
        ebooklib.ITEM_UNKNOWN: "Unknown",
        ebooklib.ITEM_IMAGE: "Image",
        ebooklib.ITEM_STYLE: "Stylesheet",
        ebooklib.ITEM_SCRIPT: "Script",
        ebooklib.ITEM_NAVIGATION: "Navigation",
        ebooklib.ITEM_VECTOR: "Vector Graphics",
        ebooklib.ITEM_FONT: "Font",
        ebooklib.ITEM_VIDEO: "Video",
        ebooklib.ITEM_AUDIO: "Audio",
        ebooklib.ITEM_DOCUMENT: "HTML Document",
        ebooklib.ITEM_COVER: "Cover Image",
        ebooklib.ITEM_SMIL: "SMIL Document"
    }

    def __init__(self, file_path):
        super().__init__(file_path)

    def fix_epub(self, input_path, output_path):
        # Create a temporary directory for extraction
        temp_dir = os.path.join(tempfile.gettempdir(), 'epub_fix')
        os.makedirs(temp_dir, exist_ok=True)

        try:
            # Extract EPUB
            with zipfile.ZipFile(input_path, 'r') as z:
                z.extractall(temp_dir)
                # Get list of all files in archive for validation
                archive_files = [f.replace('\\', '/') for f in z.namelist()]

            # Find content.opf
            opf_path = os.path.join(temp_dir, 'OEBPS', 'content.opf')
            if not os.path.exists(opf_path):
                print("Error: content.opf not found!")
                return False

            # Detect actual folder names in OEBPS
            folder_map = {}
            oebps_path = os.path.join(temp_dir, 'OEBPS')
            if os.path.exists(oebps_path):
                for item in os.listdir(oebps_path):
                    if os.path.isdir(os.path.join(oebps_path, item)):
                        folder_map[item.lower()] = item
            print(f"Detected folders: {folder_map}")

            # Parse content.opf
            tree = ET.parse(opf_path)
            root = tree.getroot()
            ns = {'opf': 'http://www.idpf.org/2007/opf'}
            manifest = root.find('.//opf:manifest', ns)
            fixed = False
            removed = False

            # Process each item in manifest
            items_to_remove = []
            for item in manifest.findall('opf:item', ns):
                href = item.get('href')
                if not href:
                    continue

                # Construct full path relative to OEBPS
                full_path = os.path.join('OEBPS', href).replace('\\', '/')
                # Check if file exists in archive
                if full_path not in archive_files:
                    print(f"Removing missing href: {href}")
                    items_to_remove.append(item)
                    removed = True
                    continue

                # Fix folder case in href
                parts = href.split('/')
                if len(parts) > 1:
                    folder = parts[0]
                    folder_lower = folder.lower()
                    if folder_lower in folder_map:
                        actual_folder = folder_map[folder_lower]
                        if folder != actual_folder:
                            new_href = href.replace(folder, actual_folder, 1)
                            item.set('href', new_href)
                            print(f"Updated href: {href} -> {new_href}")
                            fixed = True
                        else:
                            print(f"No case update needed for href: {href}")
                    else:
                        print(f"Folder not found in map for href: {href}")
                else:
                    print(f"No folder in href, skipping case fix: {href}")

            # Remove items marked for deletion
            for item in items_to_remove:
                manifest.remove(item)

            if not fixed and not removed:
                print("No href updates or removals needed in content.opf")
            elif fixed:
                print("Updated hrefs in content.opf")
            if removed:
                print("Removed missing hrefs from content.opf")

            # Save updated content.opf
            tree.write(opf_path)

            # Rebuild EPUB
            with zipfile.ZipFile(output_path, 'w') as z:
                # Add mimetype first, uncompressed
                mimetype_path = os.path.join(temp_dir, 'mimetype')
                if os.path.exists(mimetype_path):
                    z.write(mimetype_path, 'mimetype', compress_type=zipfile.ZIP_STORED)
                # Add other files
                for root, _, files in os.walk(temp_dir):
                    for file in files:
                        if file != 'mimetype':
                            file_path = os.path.join(root, file)
                            arcname = os.path.relpath(file_path, temp_dir)
                            z.write(file_path, arcname, compress_type=zipfile.ZIP_DEFLATED)

            print(f"Fixed EPUB saved as {output_path}")
            return True

        finally:
            # Clean up
            if os.path.exists(temp_dir):
                shutil.rmtree(temp_dir)

    def extract_text(self):
        print(f"Extracting EPUB file: {self.file_path}")
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")  # Suppress warnings from ebooklib
            try:
                book = epub.read_epub(self.file_path, {'ignore_ncx': True})
                return self.read_epub_content(book)
            except Exception as e:
                print(f'Fail to read epub file{self.file_path}: {e}')
                folder = os.path.split(self.file_path)[0]
                file_name = os.path.splitext(os.path.basename(self.file_path))[0]
                temp_file = os.path.join(folder, f"{file_name}-fixed.epub")
                print(f"Temporary file path: {temp_file}")

                try:
                    self.fix_epub(self.file_path, temp_file)
                    book = epub.read_epub(temp_file, {'ignore_ncx': True})
                    return self.read_epub_content(book)
                except Exception as e:
                    print(f'Fail to read epub file{temp_file}: {e}')
                    raise
                # finally:
                #     if os.path.exists(temp_file):
                #         os.remove(temp_file)

        return ''

    def read_epub_content(self, book) -> str:
        raw = ""
        for item in book.get_items():
            item_type = item.get_type()

            if item_type == ebooklib.ITEM_DOCUMENT:
                body_content = item.get_body_content()
                # Decode only if it's a bytes object
                if isinstance(body_content, bytes):
                    raw += body_content.decode('utf-8')
                else:
                    raw += body_content

        text = super().clean_html(raw)
        return text

class Azw3Extractor(TextExtractor):
    def __init__(self, file_path):
        super().__init__(file_path)
        if not file_path.lower().endswith('.azw3'):
            raise ValueError(f"File {file_path} is not an AZW3 file")

    def extract_text(self) -> str:
        """
        Extract text content from an AZW3 file by unpacking it and processing its HTML parts.
        Returns the cleaned text content as a string.
        """
        temp_dir = tempfile.mkdtemp(prefix='azw3_extract_')
        try:
            # Unpack AZW3 using mobi
            print(f"Unpacking AZW3 file: {self.file_path} to {temp_dir}")
            tempdir, _ = mobi_extract(self.file_path)
            html_content = []
            for root, _, files in os.walk(tempdir):
                for file in files:
                    if file.endswith(('.html', '.xhtml')):
                        file_path = os.path.join(root, file)
                        try:
                            html = self.read_text(file_path)
                            cleaned_text = self.clean_html(html)
                            if cleaned_text.strip():
                                html_content.append(cleaned_text)
                        except Exception as e:
                            print(f"Error processing {file_path}: {e}")
            if not html_content:
                print(f"No valid HTML content found in {self.file_path}")
                return ""
            return "\n\n".join(html_content).strip()
        except Exception as e:
            print(f"Failed to extract text from {self.file_path}: {e}")
            return ""
        finally:
            if os.path.exists(temp_dir):
                shutil.rmtree(temp_dir, ignore_errors=True)


class FileProcessor():
    def __init__(self, file_path):
        self.file_path = file_path

    def process(self):
        folder, file = os.path.split(self.file_path)
        file_name = os.path.splitext(file)[0]
        destination = os.path.join(folder, f"{file_name}.txt")

        print(f"Extracting text from {self.file_path}")

        extractor = self.get_extractor(self.file_path)
        extractor.extract_text_to(destination)
        extractor.open_with_notepad(destination)

    def get_extractor(self, file_path):
        print(f'Get extractor for {file_path}')
        if file_path.endswith(".pdf"):
            extractor = PDFExtractor(file_path)
        elif file_path.endswith(".mobi"):
            extractor = MobiExtractor(file_path)
        elif file_path.endswith(".epub"):
            extractor = EpubExtractor(file_path)
        elif file_path.endswith(".azw3"):
            extractor = Azw3Extractor(file_path)
        else:
            raise ValueError("Unsupported file format")
        return extractor

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Extract text from an ebook file.")
    parser.add_argument("--file_path", help="Path to the ebook file", type=str, nargs='?')
    args = parser.parse_args()

    print(args)

    file_path = args.file_path

    print(f"Extracting text from {file_path}")
    processor = FileProcessor(file_path)
    processor.process()
