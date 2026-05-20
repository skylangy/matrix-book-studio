from filmSettings import FilmSettings

from typing import List

class CommandBuilderContext:
    """
    Context manager for command execution.
    It provides a way to execute commands with a specific context.
    """

    def __init__(self, settings: FilmSettings = None):
        self.settings = settings if settings else FilmSettings()
        self.output = settings.output

        self.input_index = 0
        self.current_label = "[base]"
        self.current_audio_label = None
        self.inputs = []
        self.filters = []

        self.connect_tracks = []
        self.audio_tracks = []
        self.overlays_tracks = []

        self.pre_track = None
        self.overwrite = True

class FilterResult:
    def __init__(self, filter: str, label: str):
        """
        Initialize the FilterResult.

        Args:
            filter_str (str): The filter string to be used in the command.
            label (str): The label for the filter.
        """
        self.filter = filter
        self.label = label