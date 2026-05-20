from enum import Enum
from PIL import ImageFont
from fonts import Fonts
import subprocess

class Size:

    def __init__(self, width: int, height: int):
        self.width = width
        self.height = height

    def __repr__(self):
        return f'Size(width={self.width}, height={self.height})'

class Location:

    def __init__(self, x: int, y: int):

        self.x = x
        self.y = y

    def __repr__(self):
        return f'Location(x={self.x}, y={self.y})'

class TextModel:

    def __init__(self, text: str,
                 fontsize: int = 24,
                 fontcolor: str = 'white',
                 fontname: str = 'MicrosoftYaHei',
                 alpha: float = 1.0,
                 show_shadow: bool = False,
                 shadowcolor: str = 'black',
                 shadowx: int = 2,
                 shadowy: int = 2):
        self.text = text
        self.fontsize = fontsize
        self.fontcolor = fontcolor
        self.fontname = fontname
        self.alpha = alpha
        self.show_shadow = show_shadow
        self.shadowcolor = shadowcolor
        self.shadowx = shadowx
        self.shadowy = shadowy

    def __repr__(self):
        return (f'TextModel(text={self.text}, '
                f'fontsize={self.fontsize}, fontcolor={self.fontcolor}, '
                f'fontfile={self.fontname})')

    def escape_text(self) -> str:
        return self.text.replace(':', '\\:').replace('\'', '\\\'')

    def get_text_size(self) -> Size:
        font_path = Fonts.resolve(self.fontname).escape_path()
        font = ImageFont.truetype(font_path, self.fontsize)
        size = font.getbbox(self.text)
        return Size(size[2] - size[0], size[3] - size[1])

class Transitions:


    ALL = [
        'fade', 'wipeleft', 'wiperight', 'wipeup', 'wipedown',
        'slideleft', 'slideright', 'slideup', 'slidedown',
        'circlecrop', 'rectcrop', 'distance', 'fadeblack', 'fadewhite',
        'radial', 'smoothleft', 'smoothright', 'smoothup', 'smoothdown',
        'circleopen', 'circleclose', 'vertopen', 'vertclose',
        'horzopen', 'horzclose', 'dissolve', 'pixelize', 'diagtl',
        'diagtr', 'diagbl', 'diagbr', 'hlslice', 'hrslice', 'vuslice', 'vdslice'
    ]

    @classmethod
    def is_supported(cls, name: str) -> bool:
        return name in cls.ALL

    @classmethod
    def list(cls) -> list:
        return cls.ALL

    @classmethod
    def validate_or_raise(cls, name: str):
        if name not in cls.ALL:
            raise ValueError(f'Transition "{name}" is not supported by FFmpeg xfade. Use one of: {", ".join(cls.ALL)}')

class Align(Enum):
    LEFT = 'left'
    CENTER = 'center'
    RIGHT = 'right'
    TOP = 'top'
    MIDDLE = 'middle'
    BOTTOM = 'bottom'
    TOP_LEFT = 'top_left'
    TOP_RIGHT = 'top_right'
    BOTTOM_LEFT = 'bottom_left'
    BOTTOM_RIGHT = 'bottom_right'
    CENTER_LEFT = 'center_left'
    CENTER_RIGHT = 'center_right'

class Margin:
    def __init__(self, left: int = 0, right: int = 0, top: int = 0, bottom: int = 0):
        self.left = left
        self.right = right
        self.top = top
        self.bottom = bottom

    def __repr__(self):
        return (f'Margin(left={self.left}, right={self.right}, '
                f'top={self.top}, bottom={self.bottom})')

    def copy(self):
        return Margin(self.left, self.right, self.top, self.bottom)

class Alignment:

    @staticmethod
    def get_location(align: Align, canvas_size: Size, component_size: Size, margin: Margin, max_width: int = 0) -> Location:

        result = Location(margin.left, margin.top)

        if align == Align.LEFT:
            result = Location(margin.left, margin.top)
        elif align == Align.CENTER:
            result = Location(margin.left + (max_width - component_size.width) / 2, margin.top + (component_size.height / 2))
        elif align == Align.RIGHT:
            result = Location(margin.left + component_size.width, margin.top)
        elif align == Align.TOP_RIGHT:
            result = Location(canvas_size.width - component_size.width - margin.right, margin.top)
        return result

class MediaFile:
    def __init__(self, path: str):
        self.path = path
        self.start = 0.0
        self.duration = 0.0

    def __repr__(self):
        return f'MediaFile(path={self.path}, start={self.start}, duration={self.duration})'

    def escape_path(self) -> str:
        return self.path.replace(' ', '\\ ')

    def get_duration(self) -> float:
        '''Returns the duration of a video file in seconds as a float.'''
        result = subprocess.run(
            [
                'ffprobe',
                '-v', 'error',
                '-show_entries', 'format=duration',
                '-of', 'default=noprint_wrappers=1:nokey=1',
                self.path
            ],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        return float(result.stdout.strip())

