import re
import os
import json

class VideoFile:
    def __init__(self, file_name, full_path):
        self.file_name = file_name
        self.full_path = full_path
    
    def __str__(self):
        return f"{self.file_name} - {self.full_path}"
    
    def __repr__(self):
        return self.__str__()

class Book:
    def __init__(self):
        self.title = ''
        self.subtitle = ''
        self.author = ''
        self.summary = ''
        self.tags = []
        self.videos = []

    def load(self, info_file: str):
        with open(info_file, 'r', encoding='utf-8') as file:
            json_data = json.load(file)
            self.title = json_data.get('title', self.title)
            self.subtitle = json_data.get('subtitle', self.subtitle)
            self.author = json_data.get('author', self.author)
            self.summary = json_data.get('summary', self.summary)
            self.tags = json_data.get('tags', self.tags)
    
    def __str__(self):
        videos_str = "\n".join(f"\t- {video}" for video in self.videos)
        return f"Title: {self.title}\nSubtitle: {self.subtitle}\nAuthor: {self.author}\nSummary: {self.summary[0:50]}\nTags: {', '.join(self.tags)}\nVideos:\n{videos_str}"



class BookLoader:
    chinese_numerals = {
        '一': 1, '二': 2, '三': 3, '四': 4, '五': 5,
        '六': 6, '七': 7, '八': 8, '九': 9, '十': 10,
        '十一': 11, '十二': 12, '十三': 13, '十四': 14, '十五': 15,
        '十六': 16, '十七': 17, '十八': 18, '十九': 19, '二十': 20,
        '二十一': 21, '二十二': 22, '二十三': 23, '二十四': 24, '二十五': 25,
        '二十六': 26, '二十七': 27, '二十八': 28, '二十九': 29, '三十': 30,
        '三十一': 31, '三十二': 32, '三十三': 33, '三十四': 34, '三十五': 35,
        '三十六': 36, '三十七': 37, '三十八': 38, '三十九': 39, '四十': 40,
        '四十一': 41, '四十二': 42, '四十三': 43, '四十四': 44, '四十五': 45,
        '四十六': 46, '四十七': 47, '四十八': 48, '四十九': 49, '五十': 50,
        '五十一': 51, '五十二': 52, '五十三': 53, '五十四': 54, '五十五': 55,
        '五十六': 56, '五十七': 57, '五十八': 58, '五十九': 59, '六十': 60,
        '六十一': 61, '六十二': 62, '六十三': 63, '六十四': 64, '六十五': 65,
        '六十六': 66, '六十七': 67, '六十八': 68, '六十九': 69, '七十': 70,
        '七十一': 71, '七十二': 72, '七十三': 73, '七十四': 74, '七十五': 75,
        '七十六': 76, '七十七': 77, '七十八': 78, '七十九': 79, '八十': 80,
        '八十一': 81, '八十二': 82, '八十三': 83, '八十四': 84, '八十五': 85,
        '八十六': 86, '八十七': 87, '八十八': 88, '八十九': 89, '九十': 90,
        '九十一': 91, '九十二': 92, '九十三': 93, '九十四': 94, '九十五': 95,
        '九十六': 96, '九十七': 97, '九十八': 98, '九十九': 99, '一百': 100,
    }
    
    def load(self, path: str) -> Book:
        print(f'Loading book from: {path}')

        info_file = os.path.join(path, 'info.json')
        if not os.path.exists(info_file):
            raise FileNotFoundError(f"Book info file 'info.json' not found in {path}")
                           
        book = Book()
        book.load(info_file)
        book.videos = self.get_files(os.path.join(path, 'mp4'))
        return book
    
    def get_files(self, folder_path, extension=".mp4") -> list:
        files = []
        for root, _, filenames in os.walk(folder_path):
            for filename in filenames:
                if filename.endswith(extension):
                    full_path = os.path.join(root, filename)
                    file_name = os.path.splitext(filename)[0]
                    files.append(VideoFile(file_name, full_path) )
                
        sorted_files = sorted(files, key=lambda x: self.extract_chapter_number(x.file_name))
        return sorted_files
        
    def extract_chapter_number(self, filename):
        match = re.search(r'第([一二三四五六七八九十]+)章', filename)
        if match:
            numeral = match.group(1)
            return self.chinese_numerals.get(numeral, 0)
        return 0