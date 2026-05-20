from dataclasses import dataclass

@dataclass
class Voice:
    name: str
    short_name: str
    gender: str
    locale: str
    sample_rate_hertz: str = "24000"
    voice_type: str = "Neural"

    @classmethod
    def from_dict(cls, data: dict) -> 'Voice':
        return cls(
            name=data["Name"],
            short_name=data["ShortName"],
            gender=data["Gender"],
            locale=data["Locale"]
        )

    def to_dict(self) -> dict:
        return {
            "Name": self.name,
            "ShortName": self.short_name,
            "Gender": self.gender,
            "Locale": self.locale,
            "SampleRateHertz": self.sample_rate_hertz,
            "VoiceType": self.voice_type
        }