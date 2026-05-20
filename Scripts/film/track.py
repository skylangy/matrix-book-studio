from abc import ABC, abstractmethod
from dataclasses import dataclass, field
from typing import List

from commandBuilderContext import CommandBuilderContext, FilterResult
from logger import Logger


class FilterInfo:
    input_index: int = 0
    input_label: str = None
    output_label: str = None
    filters: List[str] = []

    def __repr__(self):
        return f'Index={self.input_index}, input_label={self.input_label}, output_label={self.output_label}, filters={self.filters})'

@dataclass
class Track(ABC):
    start: float = 0.0
    duration: float = 0.0
    logger: Logger = field(default=None, init=False)
    is_overlay: bool = field(default=False, init=False)
    is_audio: bool = field(default=False, init=False)
    has_input: bool = field(default=True, init=False)
    filter_info: FilterInfo = field(default_factory=FilterInfo)

    def __post_init__(self):
        self.logger = Logger(self.__class__.__name__)
        self.filter_info = FilterInfo()

    @abstractmethod
    def get_inputs(self, context:CommandBuilderContext) -> List[str]:
        pass

    @abstractmethod
    def get_filter(self, context:CommandBuilderContext) -> FilterResult:
        pass

    def __repr__(self):
        return f'{self.__class__.__name__}(start={self.start}, duration={self.duration})'
