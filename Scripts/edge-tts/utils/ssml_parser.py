import xml.etree.ElementTree as ET
from typing import Tuple

class SsmlParser:
    @staticmethod
    def parse_ssml(ssml: str) -> Tuple[str, str]:
        """Parse SSML to extract voice and text"""
        try:
            # Parse the SSML string into an ElementTree
            root = ET.fromstring(ssml)

            # Define namespaces
            namespaces = {
                'synth': 'http://www.w3.org/2001/10/synthesis',
                'mstts': 'http://www.w3.org/2001/mstts',
                'emo': 'http://www.w3.org/2009/10/emotionml'
            }

            # Check root element is <speak>
            if root.tag != '{http://www.w3.org/2001/10/synthesis}speak':
                raise ValueError("Invalid SSML: root element must be 'speak'")

            # Find the <voice> element using namespace
            voice_elem = root.find('.//synth:voice', namespaces)
            if voice_elem is None:
                raise ValueError("No <voice> element found in SSML")

            # Extract voice name
            voice = voice_elem.get('name')
            if not voice:
                raise ValueError("Voice element is missing the 'name' attribute")

            # Extract all text within the <voice> element
            text = ''.join(voice_elem.itertext()).strip()
            if not text:
                raise ValueError("No text found within <voice> element")

            return text, voice

        except ET.ParseError:
            raise ValueError("Invalid SSML format: parsing failed")
        except Exception as e:
            raise ValueError(f"SSML parsing error: {str(e)}")