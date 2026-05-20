from abc import ABC, abstractmethod

class ISpeechService(ABC):
    @abstractmethod
    def synthesize(self, text):
        pass

    @abstractmethod
    def synthesize_ssml(self, ssml):
        pass