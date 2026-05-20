from abc import ABC, abstractmethod

class ITranscribeService(ABC):
    @abstractmethod
    def transcribe(self, audio_file):
        pass