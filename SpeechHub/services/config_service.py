from models.config import Config

class ConfigService:
    CONFIG_FILE = 'config.json'

    @staticmethod
    def load_config() -> Config:
        """Load configuration and return a Config object."""
        return Config(ConfigService.CONFIG_FILE)

    @staticmethod
    def save_config(config: Config) -> None:
        """Save configuration using a Config object."""
        config.save_config()