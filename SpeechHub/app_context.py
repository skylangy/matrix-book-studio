from services.service_collection import ServiceCollection
from services.config_service import ConfigService
from services.kokoro_speech_service import KokoroSpeechService
from services.whisper_transcribe_service import WhisperTranscribeService

class AppContext:
    def __init__(self):
        self.service_collection = ServiceCollection()
        self.config = ConfigService.load_config()
        self.initialize_services()

    def initialize_services(self):
        # Register default services with names
        self.service_collection.register_service(KokoroSpeechService, KokoroSpeechService(), name="kokoro")
        self.service_collection.register_service(WhisperTranscribeService, WhisperTranscribeService(), name="whisper")

        # Set default services
        default_speech_service_name = self.config.get('default_speech_service', 'kokoro')
        default_transcribe_service_name = self.config.get('default_transcribe_service', 'whisper')

        self.config['default_speech_service'] = default_speech_service_name
        self.config['default_transcribe_service'] = default_transcribe_service_name
        ConfigService.save_config(self.config)

    def get_service(self, service_type, name=None):
        return self.service_collection.get_service(service_type, name=name)

    def get_config(self):
        return self.config