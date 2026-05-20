import os
import platform
from typing import Optional, List, Dict
from dataclasses import dataclass

@dataclass(frozen=True)
class FontFile:
    name: str   # Human-readable font name (e.g., 'MicrosoftYaHei')
    path: str   # Full path to the .ttf/.ttc/.otf file

    def escape_path(self) -> str:

        if not self.path:
            return ''
        return self.path.replace('\\', '/').replace(':', '\\:').replace('"', '\\"')

    def __repr__(self):
        return f'FontFile(name="{self.name}", path="{self.path}")'

    def exists(self) -> bool:
        return os.path.exists(self.path)

class Fonts:
    WindowsFontPath = os.path.expandvars(r'%WINDIR%\Fonts')

    _font_extensions = {'.ttf', '.otf', '.ttc'}
    _font_map: Dict[str, FontFile] = {}
    _initialized = False

    _predefined_fonts = {
        # Windows
        'Arial': os.path.join(WindowsFontPath, 'arial.ttf'),
        'MicrosoftYaHei': os.path.join(WindowsFontPath, 'msyh.ttc'),
        'SimHei': os.path.join(WindowsFontPath, 'simhei.ttf'),
        'SimSun': os.path.join(WindowsFontPath, 'simsun.ttc'),
        'KaiTi': os.path.join(WindowsFontPath, 'simkai.ttf'),
        'FangSong': os.path.join(WindowsFontPath, 'simfang.ttf'),
        'TimesNewRoman': os.path.join(WindowsFontPath, 'times.ttf'),

        # macOS
        'PingFang': '/System/Library/Fonts/PingFang.ttc',
        'Heiti SC': '/System/Library/Fonts/STHeiti Light.ttc',

        # Linux / Cross-platform
        'DejaVuSans': '/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',
        'DejaVuSans-Bold': '/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',
        'NotoSansCJK': '/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc',
        'WenQuanYiZenHei': '/usr/share/fonts/wenquanyi/wqy-zenhei.ttc',
    }

    @classmethod
    def _get_default_font_dirs(cls) -> List[str]:
        system = platform.system()
        if system == 'Windows':
            return [cls.WindowsFontPath]
        elif system == 'Darwin':
            return [
                '/System/Library/Fonts', '/Library/Fonts', os.path.expanduser('~/Library/Fonts')
            ]
        elif system == 'Linux':
            return [
                '/usr/share/fonts', '/usr/local/share/fonts', os.path.expanduser('~/.fonts')
            ]
        return []

    @classmethod
    def _initialize(cls):
        '''Scan font directories and populate the font map.'''
        if cls._initialized:
            return

        for font_dir in cls._get_default_font_dirs():
            if not os.path.isdir(font_dir):
                continue

            for root, _, files in os.walk(font_dir):
                for file in files:
                    ext = os.path.splitext(file)[1].lower()
                    if ext in cls._font_extensions:
                        font_file = FontFile(name=os.path.splitext(file)[0], path=os.path.join(root, file))
                        if font_file.name not in cls._font_map:  # First match wins
                            cls._font_map[font_file.name] = font_file

        # Add predefined fonts
        for name, path in cls._predefined_fonts.items():
            if os.path.exists(path):
                cls._font_map[name] = FontFile(name=name, path=path)


        for name, font in cls._font_map.items():
            # print(f'Found font: {name} at {font.path}')
            setattr(cls, name, font)
        cls._initialized = True

    @classmethod
    def resolve(cls, name: str) -> Optional[FontFile]:
        '''Get full path to font file for a given name.'''
        cls._initialize()

        return cls._font_map.get(name)

    @classmethod
    def register(cls, name: str, path: str):

        cls._font_map[name] = FontFile(name=name, path=path)

    @classmethod
    def unregister(cls, name: str):
        cls._font_map.pop(name, None)

    @classmethod
    def list(cls) -> List[str]:
        cls._initialize()
        return sorted(cls._font_map.keys())

    @classmethod
    def dump(cls) -> Dict[str, str]:
        cls._initialize()
        return dict(cls._font_map)

    @classmethod
    def __getattr__(cls, name: str) -> FontFile:
        cls._initialize()
        if name in cls._font_map:
            return cls._font_map[name]
        raise AttributeError(f'Fonts has no font named "{name}"')