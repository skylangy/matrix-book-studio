FROM python:3.12-slim

WORKDIR /app

COPY app.py .
COPY services/ services/
COPY models/ models/
COPY utils/ utils/
COPY requirements.txt .

RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

RUN pip install --no-cache-dir -r requirements.txt

# Use Gunicorn to run the Flask app
ENV PORT=8000
EXPOSE ${PORT}

# Adjust the module path if app.py is in a package or uses an app factory
CMD ["sh", "-c", "gunicorn -w 4 -b 0.0.0.0:$PORT app:app"]
