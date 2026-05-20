from services.speech_service import ISpeechService

class KokoroSpeechService(ISpeechService):
    def __init__(self, kokoro_tts):
        self.kokoro_tts = kokoro_tts

    def synthesize(self, text):
        return self.kokoro_tts.synthesize(text)

    def synthesize_ssml(self, ssml):
        return self.kokoro_tts.synthesize_ssml(ssml)