FROM buildpack-deps:bullseye

WORKDIR /usr/src/app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    espeak \
    ffmpeg \
    libsndfile1 \
    libsndfile1-dev \
    python3 \
    python3-dev \
    python3-pip \
    build-essential \
    pkg-config \
    && ldconfig \
    && rm -rf /var/lib/apt/lists/*

# Set library path
ENV LD_LIBRARY_PATH=/usr/lib:/usr/local/lib

# Install Python dependencies
RUN pip3 install --no-cache-dir numpy==1.22.0
RUN pip3 install --no-cache-dir aeneas

# Verify installation
RUN python3 -c "import aeneas; print(aeneas.__version__)"

CMD ["python3"]

# Build: docker build -t aeneas:latest .