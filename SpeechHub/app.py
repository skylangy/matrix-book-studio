from flask import Flask, request, jsonify
from app_context import AppContext
from services.kokoro_speech_service import KokoroSpeechService
from services.whisper_transcribe_service import WhisperTranscribeService

# Flask app setup
app = Flask(__name__)

# Initialize application context
app_context = AppContext()

@app.route('/tts', methods=['POST'])
def text_to_speech():
    """Endpoint for text-to-speech conversion"""
    data = request.get_json()
    text = data.get('text')
    if not text:
        return jsonify({'error': 'Text is required'}), 400

    try:
        speech_service = app_context.get_service(KokoroSpeechService)
        audio_file = speech_service.synthesize(text)
        return jsonify({'audio_file': audio_file})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/transcribe', methods=['POST'])
def transcribe_audio():
    """Endpoint for audio transcription"""
    file = request.files.get('file')
    if not file:
        return jsonify({'error': 'Audio file is required'}), 400

    try:
        transcribe_service = app_context.get_service(WhisperTranscribeService)
        transcription = transcribe_service.transcribe(file)
        return jsonify({'transcription': transcription})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/ssml', methods=['POST'])
def ssml_to_speech():
    """Endpoint for SSML-based text-to-speech conversion"""
    data = request.get_json()
    ssml = data.get('ssml')
    if not ssml:
        return jsonify({'error': 'SSML is required'}), 400

    try:
        speech_service = app_context.get_service(KokoroSpeechService)
        audio_file = speech_service.synthesize_ssml(ssml)
        return jsonify({'audio_file': audio_file})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)