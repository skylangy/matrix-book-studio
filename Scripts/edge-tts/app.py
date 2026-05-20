from flask import Flask, request, send_file, jsonify, after_this_request
from services.tts_service import TtsService
from services.logger import Logger
from models.voice import Voice
from models.file import File
from utils.ssml_parser import SsmlParser
from utils.async_utils import run_async
from utils.audio import AudioConverter
from datetime import datetime, timezone
import os
import threading

app = Flask(__name__)
logger = Logger("App")
tts_service = TtsService()
audio_converter = AudioConverter(logger)


@app.route('/', methods=['GET'])
def default_route():
    """Default route for health check or API welcome message"""

    return jsonify({
        'status': 'TTS API is running',
        'message': 'Welcome to the Text-to-Speech API',
        'version': '1.0',
        'current_date': datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S UTC')
    })

@app.route('/tts/ssml', methods=['POST'])
def speech_ssml():
    """SSML-based endpoint for speech synthesis"""
    try:
        logger.info("TTS with SSML request")
        if request.mimetype != "application/ssml+xml":
            return jsonify({'error': 'Content-Type must be application/ssml+xml'}), 400

        output_format = request.headers.get('X-Microsoft-OutputFormat')
        is_valid, error_msg = validate_output_format(output_format)
        if not is_valid:
            return jsonify({'error': error_msg}), 400

        ssml = request.data.decode('utf-8')
        text, voice = SsmlParser.parse_ssml(ssml)
        voice = voice or tts_service.default_voice

        temp_file = File(tts_service.get_output_file_path())
        run_async(tts_service.generate_speech, text, voice, temp_file.path)

        wav_file = temp_file
        if temp_file.path.endswith('.mp3'):
            wav_file = File(audio_converter.convert_to_wav(temp_file.path))

        @after_this_request
        def cleanup(response):
            def delayed():
                try:
                    temp_file.delete()
                    logger.info(f"Deleted temporary file: {temp_file.path}")

                    if wav_file != temp_file:
                        wav_file.delete()
                        logger.info(f"Deleted converted file: {wav_file.path}")
                except Exception as e:
                    logger.error(f"Error cleaning up files: {e}")
            threading.Timer(5, delayed).start()
            return response

        return send_audio_response(wav_file.path)

    except Exception as e:
        logger.error(f"SSML endpoint error: {e}")
        return jsonify({'error': str(e)}), 500

@app.route('/tts/text', methods=['POST'])
def speech_text():
    """Plain text endpoint for speech synthesis"""
    try:
        logger.info("TTS with plain text request")
        if request.content_type != 'application/json':
            return jsonify({'error': 'Content-Type must be application/json'}), 400

        output_format = request.headers.get('X-Microsoft-OutputFormat')
        is_valid, error_msg = validate_output_format(output_format)
        if not is_valid:
            return jsonify({'error': error_msg}), 400

        data = request.get_json()
        if not data or 'text' not in data:
            return jsonify({'error': 'Request body must contain a "text" field'}), 400

        text = data['text']
        voice = request.args.get('voice') or data.get('voice') or tts_service.default_voice

        temp_file = File(tts_service.get_output_file_path())
        run_async(tts_service.generate_speech, text, voice, temp_file.path)

        @after_this_request
        def cleanup(response):
            def delayed():
                try:
                    temp_file.delete()
                except Exception as e:
                    logger.error(f"Error deleting file: {e}")
            threading.Timer(3, delayed).start()
            return response

        return send_audio_response(temp_file.path)

    except Exception as e:
        logger.error(f"Text endpoint error: {e}")
        return jsonify({'error': str(e)}), 500

@app.route('/voices/list', methods=['GET'])
def list_voices():
    """List available voices"""
    try:
        logger.info("Listing available voices")
        voices_raw = run_async(tts_service.list_voices)
        voices = [Voice.from_dict(v).to_dict() for v in voices_raw]
        return jsonify(voices)
    except Exception as e:
        logger.error(f"Error listing voices: {e}")
        return jsonify({'error': str(e)}), 500

@app.route('/clean', methods=['GET'])
def clean_temp_files():
    """Manually trigger temp file cleanup"""
    removed = 0
    for f in os.listdir(tts_service.output_dir):
        file_path = os.path.join(tts_service.output_dir, f)
        if os.path.isfile(file_path) and f.startswith('tts_'):
            try:
                File.try_delete(file_path)
                removed += 1
            except Exception as e:
                logger.error(f"Clean up failed to remove {file_path}: {e}")

    logger.info(f"Cleaned up {removed} temporary files.")
    return jsonify({'removed_files_count': removed})

def validate_output_format(output_format):
    if not output_format:
        return False, 'X-Microsoft-OutputFormat header is required'
    if output_format not in tts_service.supported_formats:
        return False, f'Unsupported output format. Supported formats: {list(tts_service.supported_formats.keys())}'
    return True, None

def send_audio_response(file_path: str):
    """Helper to send audio with consistent headers"""
    response = send_file(
        file_path,
        mimetype='audio/wav',
        as_attachment=True,
        download_name='speech.wav'
    )
    response.headers['X-Request-Id'] = os.urandom(16).hex()
    response.headers['Date'] = datetime.now(timezone.utc).strftime('%a, %d %b %Y %H:%M:%S GMT')
    return response

if __name__ == '__main__':
    port = int(os.environ.get("PORT", 8000))
    logger.info(f"Starting TTS API server at {port}.")
    app.run(host='0.0.0.0', port=port)