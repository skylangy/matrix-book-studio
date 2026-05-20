import os
import subprocess
import logging
import argparse
from pathlib import Path
from typing import List, Optional, Tuple
from PIL import ImageFont

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

class ImageSegment:
    """A class to represent an image segment with its duration."""

    def __init__(self, image_path: Path, duration: float):
        """
        Initialize the ImageSegment.

        Args:
            image_path (Path): Path to the image file.
            duration (float): Duration for which the image is displayed in the video.
        """
        self.image_path = image_path
        self.duration = duration

    def __repr__(self):
        return f"ImageSegment(image_path={self.image_path}, duration={self.duration})"

class Size:
    """A class to represent a size with width and height."""

    def __init__(self, width: int, height: int):
        """
        Initialize the Size.

        Args:
            width (int): Width of the size.
            height (int): Height of the size.
        """
        self.width = width
        self.height = height

    def __repr__(self):
        return f"Size(width={self.width}, height={self.height})"

class Location:
    """A class to represent a location with x and y coordinates."""

    def __init__(self, x: int, y: int):
        """
        Initialize the Location.

        Args:
            x (int): X coordinate.
            y (int): Y coordinate.
        """
        self.x = x
        self.y = y

    def __repr__(self):
        return f"Location(x={self.x}, y={self.y})"

class VideoCreator:
    """A class to create videos from images with fade transitions using FFmpeg."""

    def __init__(self,
                 input_filelist: str = None,
                 input_dir: str = None,
                 overlay_video: str = None,
                 output_file: str = 'output.mp4',
                 duration: float = 5.0,
                 title: str = None,
                 subtitle: str = None,
                 overlay_transparency: float = 1.0,
                 transition_duration: float = 1.0,
                 use_gpu: bool = False,
                 log_level: str = 'debug'):
        """
        Initialize the VideoCreator.

        Args:
            input_filelist (str): Path to a file list containing image paths (optional).
            input_dir (str): Directory containing input images (optional, used if input_filelist is None).
            output_file (str): Path to the output video file.
            duration (float): Duration each image is displayed (seconds).
            transition_duration (float): Duration of the fade transition (seconds).
            use_gpu (bool): Whether to use GPU acceleration (NVENC) if available.
            log_level (str): FFmpeg log level (quiet, panic, fatal, error, warning, info, verbose, debug, trace).
        """
        if input_filelist is None and input_dir is None:
            raise ValueError("Either input_filelist or input_dir must be provided.")
        self.input_filelist = Path(input_filelist) if input_filelist else None
        self.input_dir = Path(input_dir) if input_dir else None
        self.overlay_video = Path(overlay_video) if overlay_video else None
        self.overlay_transparency = overlay_transparency
        self.output_file = Path(output_file)
        self.title = title
        self.subtitle = subtitle
        self.duration = duration
        self.transition_duration = transition_duration
        self.use_gpu = use_gpu
        self.log_level = log_level.lower()
        self.ffmpeg_bin = self._find_ffmpeg()
        self.gpu_available = self._check_gpu_support() if self.use_gpu else False
        self.images = self._load_images()
        self._validate_log_level()
        if self.input_dir and not self.input_filelist:
            self.input_filelist = Path('images.txt')
            self._create_filelist(self.input_filelist)

        self.title_font = 'C\\:/Windows/Fonts/msyh.ttc'
        self.subtitle_font = 'C\\:/Windows/Fonts/msyh.ttc'
        self.title_font_size = 24
        self.subtitle_font_size = 18
        self.title_font_color = 'white'
        self.subtitle_font_color = 'white'
        self.title_left_margin = 20
        self.title_position = Location(90, 60)
        self.subtitle_position = Location(20, 100)
        self.logo_path = Path(r'I:\Source\overlays\book-logo.png')
        self.logo_size = Size(72, 72)
        self.logo_position = Location(1808, 40)
        self.prev_video_label = 'v_with_overlay'

    def _find_ffmpeg(self) -> str:
        """Find FFmpeg executable or raise an error if not found."""
        try:
            result = subprocess.run(['ffmpeg', '-version'], capture_output=True, text=True, check=True)
            logger.info("FFmpeg found: %s", result.stdout.split('\n')[0])
            return 'ffmpeg'
        except (subprocess.CalledProcessError, FileNotFoundError):
            logger.error("FFmpeg not found. Please install FFmpeg and ensure it's in your PATH.")
            raise FileNotFoundError("FFmpeg is required but not found.")

    def _check_gpu_support(self) -> bool:
        """Check if NVIDIA GPU acceleration (NVENC) is supported."""
        try:
            result = subprocess.run([self.ffmpeg_bin, '-encoders'], capture_output=True, text=True, check=True)
            if 'h264_nvenc' in result.stdout:
                logger.info("NVIDIA NVENC (h264_nvenc) is supported.")
                return True
            else:
                logger.warning("NVIDIA NVENC not supported. Falling back to CPU encoding.")
                return False
        except subprocess.CalledProcessError:
            logger.warning("Error checking GPU support. Falling back to CPU encoding.")
            return False

    def _validate_log_level(self) -> None:
        """Validate the provided FFmpeg log level."""
        valid_levels = ['quiet', 'panic', 'fatal', 'error', 'warning', 'info', 'verbose', 'debug', 'trace']
        if self.log_level not in valid_levels:
            logger.error("Invalid FFmpeg log level: %s. Valid levels: %s", self.log_level, valid_levels)
            raise ValueError(f"Invalid FFmpeg log level: {self.log_level}")

    def _load_images(self) -> List[Path]:
        """Load image files from a file list or directory."""
        if self.input_filelist:
            try:
                with open(self.input_filelist, 'r', encoding='utf-8') as f:
                    lines = f.readlines()
                images = []
                for line in lines:
                    line = line.strip()
                    if line.startswith('file '):
                        path = line[5:].strip().strip("'").strip('"')
                        image_path = Path(path)
                        if image_path.exists():
                            images.append(image_path)
                        else:
                            logger.warning("Image path does not exist: %s", path)
                if not images:
                    logger.error("No valid image paths found in file list: %s", self.input_filelist)
                    raise ValueError("No valid image paths in file list.")
                logger.info("Loaded %d images from file list: %s", len(images), self.input_filelist)
                return images
            except FileNotFoundError:
                logger.error("File list not found: %s", self.input_filelist)
                raise
            except UnicodeDecodeError as e:
                logger.error("Failed to decode file list with UTF-8: %s", str(e))
                raise
        else:
            image_extensions = ('.jpg', '.jpeg', '.png')
            images = sorted(
                [f for f in self.input_dir.iterdir() if f.suffix.lower() in image_extensions],
                key=lambda x: x.name
            )
            if not images:
                logger.error("No images found in directory: %s", self.input_dir)
                raise ValueError("No valid images found in the input directory.")
            logger.info("Found %d images in %s", len(images), self.input_dir)
            return images

    def _create_filelist(self, filelist_path: Path) -> None:
        """Generate a file list from images in input_dir using UTF-8 encoding."""
        if not self.input_dir:
            raise ValueError("input_dir must be provided to create a file list.")
        try:
            with open(filelist_path, 'w', encoding='utf-8') as f:
                for image in self.images:
                    logger.info("Adding image to file list: %s", image.resolve())
                    f.write(f"file '{image.resolve()}'\n")
                    f.write(f"duration {self.duration}\n")
            logger.info("Created file list with UTF-8 encoding: %s", filelist_path)
        except UnicodeEncodeError as e:
            logger.error("Failed to encode file list with UTF-8: %s", str(e))
            raise

    def _build_ffmpeg_command(self) -> Tuple[List[str], float]:
        """Build the FFmpeg command for creating a video with crossfade transitions."""
        if len(self.images) < 1:
            raise ValueError("At least one image is required.")

        segments = self._load_image_segments()

        if not segments:
            logger.info("No valid image segments found")
            return [], 0.0

        transition_duration = self.transition_duration
        total_duration = sum(s.duration for s in segments) - transition_duration * (len(segments) - 1)
        logger.info(f"Total video duration: {total_duration:.2f}s")

        cmd = [self.ffmpeg_bin, '-y', '-loglevel', self.log_level]

        for seg in segments:
            cmd.extend(['-loop', '1', '-t', str(seg.duration), '-i', str(seg.image_path)])

        overlay_input_index = None
        if self.overlay_video and Path(self.overlay_video).exists():
            overlay_input_index = len(segments)
            cmd.extend(['-stream_loop', '-1', '-i', str(self.overlay_video)])

        if self.logo_path and self.logo_path.exists():
            self.logo_input_index = len(segments) + (1 if overlay_input_index is not None else 0)
            cmd.extend(['-i', str(self.logo_path)])

        prev, filter_complex = self._build_transition_filters(segments, transition_duration, overlay_input_index)

        cmd.extend([
            '-filter_complex', ';'.join(filter_complex),
            '-map', f'[{prev}]',
            '-c:v', 'h264_nvenc' if self.use_gpu else 'libx264',
            '-r', '30',
            '-pix_fmt', 'yuv420p',
            '-t', str(total_duration),
            str(self.output_file)
        ])

        return cmd, total_duration

    def _build_transition_filters(self,
                                  segments,
                                  transition_duration,
                                  overlay_input_index=None,
                                  overlay_position="0:0"):
        label_map = []
        filter_complex = []

        # Step 1: Format and scale all image inputs
        for i in range(len(segments)):
            label = f'v{i}'
            label_map.append(label)
            filter_complex.append(f'[{i}:v]format=yuv420p,scale=1920:1080[{label}]')

        # Step 2: Apply xfade transitions between images
        prev = label_map[0]
        offset = segments[0].duration - transition_duration
        for i in range(1, len(label_map)):
            curr = label_map[i]
            out = f'xf{i}'
            filter_complex.append(
                f'[{prev}][{curr}]xfade=transition=fade:duration={transition_duration}:offset={offset:.2f}[{out}]')
            prev = out
            if i < len(segments) - 1:
                offset += segments[i].duration - transition_duration

        # Step 3: Optional overlay
        if overlay_input_index is not None:
            logger.info(f"Adding overlay video from input index {overlay_input_index}")
            overlay_label = 'overlay_src'
            filter_complex.append(f'[{overlay_input_index}:v]format=rgba,colorchannelmixer=aa={self.overlay_transparency},scale=1920:1080[{overlay_label}]')
            out = 'v_with_overlay'
            filter_complex.append(f'[{prev}][{overlay_label}]overlay={overlay_position}[{out}]')
            prev = out

        # Step 4: Optional drawtext overlay
        render_text = self._render_text_overlay()
        if render_text:
            logger.info(f"Applying text overlay {render_text}")
            out = 'vfinal'
            filter_complex.append(f'[{prev}]{render_text}[{out}]')
            prev = out

        logo_overlay = self._render_logo_overlay(prev)
        if logo_overlay:
            logger.info(f"Applying logo overlay {logo_overlay}")
            filter_complex.extend(logo_overlay.split(";"))
            prev = 'with_logo'

        return prev, filter_complex

    def _load_image_segments(self) -> List[ImageSegment]:
        segments = []
        if self.input_filelist and self.input_filelist.exists():
            try:
                with open(self.input_filelist, 'r', encoding='utf-8') as f:
                    lines = f.readlines()

                i = 0
                while i < len(lines):
                    line = lines[i].strip()
                    logger.info(f"Processing line {i+1}: {line}")
                    if line.startswith('file '):
                        path = line[5:].strip().strip("'").strip('"')
                        image_path = Path(path)

                        duration = self.duration
                        if i + 1 < len(lines) and lines[i + 1].strip().startswith('duration '):
                            try:
                                duration = float(lines[i + 1].strip().split(' ')[1])
                                i += 1
                            except (ValueError, IndexError):
                                logger.warning(f"Invalid duration at line {i+2}, using default")

                        if image_path.exists():
                            segments.append(ImageSegment(image_path, duration))
                        else:
                            logger.warning(f"Image not found: {path}")
                    i += 1

                if segments:
                    self.segments = segments
                    logger.info(f"Loaded {len(segments)} image segments")

            except Exception as e:
                logger.error(f"Error parsing input file: {str(e)}")
                logger.info("Falling back to fixed duration.")
        return segments

    def _render_text_overlay(self) -> str:
        """Render a text overlay using FFmpeg."""
        def _escape_ffmpeg_text(text: str) -> str:
            return text.replace(":", "\\:").replace("'", "\\'")

        # Create a temporary file for the text overlay
        title_size = self.get_text_size(self.title, self.title_font, self.title_font_size) if self.title else Size(0, 0)
        subtitle_size = self.get_text_size(self.subtitle, self.subtitle_font, self.subtitle_font_size) if self.subtitle else Size(0, 0)
        title_x = self.title_left_margin + max( title_size.width, subtitle_size.width) /2 - title_size.width / 2
        subtitle_x = self.title_left_margin + max( title_size.width, subtitle_size.width) /2 - subtitle_size.width / 2

        if self.title:
            escaped_title = _escape_ffmpeg_text(self.title)
            render_title = (
                f"drawtext=fontfile='{self.title_font}':"
                f"text='{escaped_title}':"
                f"fontcolor={self.title_font_color}:"
                f"fontsize={self.title_font_size}:"
                f"x={title_x}:"
                f"y={self.title_position.y}:"
                f"shadowcolor=black:shadowx=2:shadowy=2"
            )

        if self.subtitle:
            escaped_subtitle = _escape_ffmpeg_text(self.subtitle)
            render_subtitle = (
                f"drawtext=fontfile='{self.subtitle_font}':"
                f"text='{escaped_subtitle}':"
                f"fontcolor={self.subtitle_font_color}:"
                f"fontsize={self.subtitle_font_size}:"
                f"x={subtitle_x}:"
                f"y={self.subtitle_position.y}:"
                f"shadowcolor=black:shadowx=2:shadowy=2"
            )

        result = render_title
        if render_subtitle:
            result += f",{render_subtitle}"

        logger.info("Rendered text overlay: %s", result)
        return result

    def _render_logo_overlay(self, input_label: str) -> Optional[str]:
        """
        Generate FFmpeg filter string to scale and overlay a logo image.

        Returns:
            Optional[str]: FFmpeg filter string or None.
        """
        if not self.logo_path or not Path(self.logo_path).exists():
            logger.warning("Logo path is not set or the file does not exist.")
            return None

        logger.info("Rendering logo overlay from index: %s", self.logo_input_index)

        logo_input_index = self.logo_input_index
        logo_label = "logo_scaled"

        if self.logo_size.width > 0 and self.logo_size.height > 0:
            scale_filter = f"[{logo_input_index}:v]scale={self.logo_size.width}:{self.logo_size.height}[{logo_label}]"
        else:
            logo_label = f"{logo_input_index}:v"
            scale_filter = None

        overlay_filter = f"[{input_label}][{logo_label}]overlay=x={self.logo_position.x}:y={self.logo_position.y}[with_logo]"

        if scale_filter:
            return f"{scale_filter};{overlay_filter}"
        else:
            return overlay_filter

    def get_text_size(self, text, font_path, font_size) -> Size:
        font = ImageFont.truetype(font_path, font_size)
        size = font.getbbox(text)
        return Size(size[2] - size[0], size[3] - size[1])

    def create_video(self) -> None:
        """Execute the FFmpeg command to create the video and log FFmpeg output."""
        try:
            cmd, total_duration = self._build_ffmpeg_command()
            logger.info("Creating video with duration %.2f seconds: %s", total_duration, self.output_file)
            logger.info("FFmpeg command: %s", " ".join(cmd))
            logger.info("Starting video creation...")

            # Run FFmpeg and capture output
            process = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                check=True,
                encoding='utf-8'
            )


            # Process FFmpeg stderr output
            for line in process.stderr.splitlines():
                line = line.strip()
                if not line:
                    continue
                if '[fatal]' in line or '[panic]' in line:
                    logger.critical("FFmpeg: %s", line)
                elif '[error]' in line:
                    logger.error("FFmpeg: %s", line)
                elif '[warning]' in line:
                    logger.warning("FFmpeg: %s", line)
                elif '[info]' in line:
                    logger.info("FFmpeg: %s", line)
                elif '[verbose]' in line or '[debug]' in line or '[trace]' in line:
                    logger.debug("FFmpeg: %s", line)
                else:
                    logger.info("FFmpeg: %s", line)

            logger.info("Video created successfully: %s", self.output_file)
            if process.stdout:
                logger.debug("FFmpeg stdout: %s", process.stdout)

        except subprocess.CalledProcessError as e:
            # for line in e.stderr.splitlines():
            #     line = line.strip()
            #     if not line:
            #         continue
            #     if '[fatal]' in line or '[panic]' in line:
            #         logger.critical("FFmpeg: %s", line)
            #     elif '[error]' in line:
            #         logger.error("FFmpeg: %s", line)
            #     else:
            #         logger.error("FFmpeg: %s", line)
            # logger.error("FFmpeg failed with exit code %d", e.returncode)
            logger.error("FFmpeg command failed with exit code %d", e.returncode)
            # raise RuntimeError("Failed to create video.") from e
        except Exception as e:
            logger.error("Unexpected error: %s", str(e))
            raise
        finally:
            logger.info("Video creation process completed.")
            # if self.input_dir and self.input_filelist and self.input_filelist.exists():
            #     try:
            #         self.input_filelist.unlink()
            #         logger.info("Cleaned up temporary file list: %s", self.input_filelist)
            #     except Exception as e:
            #         logger.warning("Failed to clean up file list: %s", str(e))

def parse_arguments() -> argparse.Namespace:
    """Parse command-line arguments."""
    parser = argparse.ArgumentParser(description="Create a video from images with fade transitions using FFmpeg.")
    parser.add_argument(
        '--input-filelist',
        type=str,
        default=None,
        help='Path to a file list containing image paths (default: None)'
    )
    parser.add_argument(
        '--input-dir',
        type=str,
        default=None,
        help='Directory containing input images (used if input-filelist is not provided)'
    )
    parser.add_argument(
        '--output-file',
        type=str,
        default='output.mp4',
        help='Output video file path (default: output.mp4)'
    )
    parser.add_argument(
        '--overlay-file',
        type=str,
        default=None,
        help='Overlay video file path (default: None)'
    )
    parser.add_argument(
        '--title',
        type=str,
        default=None,
        help='Video title (default: None)'
    )
    parser.add_argument(
        '--subtitle',
        type=str,
        default=None,
        help='Video subtitle (default: None)'
    )
    parser.add_argument(
        '--overlay-transparency',
        type=float,
        default=1.0,
        help='Overlay video transparency (default: 1.0, range: 0.0 to 1.0, where 1.0 is fully opaque and 0.0 is fully transparent)'
    )
    parser.add_argument(
        '--duration',
        type=float,
        default=5.0,
        help='Duration per image in seconds (default: 5.0)'
    )
    parser.add_argument(
        '--transition-duration',
        type=float,
        default=1.0,
        help='Duration of fade transition in seconds (default: 1.0)'
    )
    parser.add_argument(
        '--use-gpu',
        action='store_true',
        help='Enable GPU acceleration with NVENC (default: False)'
    )
    parser.add_argument(
        '--log-level',
        type=str,
        default='info',
        help='FFmpeg log level: quiet, panic, fatal, error, warning, info, verbose, debug, trace (default: info)'
    )
    return parser.parse_args()

def main():
    """
    Main function to run the video creation process.
    python videoCreator.py --input-dir "I:\Source\封面\红楼梦\video" --output-file "I:\Source\封面\红楼梦\video\output.mp4" --duration 5.0 --transition-duration 1.0 --use-gpu
    python videoCreator.py --input-dir "I:\Source\封面\红楼梦\video" --output-file "I:\Source\封面\红楼梦\video\output.mp4" --use-gpu
    python videoCreator.py --input-dir "I:\Source\封面\红楼梦\video" --output-file "I:\Source\封面\红楼梦\video\output.mp4" --overlay-file "I:\Source\overlays\overlay-1.mp4" --use-gpu
    python videoCreator.py --input-dir "I:\Source\封面\红楼梦\video" --output-file "I:\Source\封面\红楼梦\video\output.mp4" --overlay-file "I:\Source\overlays\overlay-1.mov" --use-gpu
    python videoCreator.py --input-dir "I:\Source\封面\红楼梦\video" --output-file "I:\Source\封面\红楼梦\video\output.mp4" --overlay-file "I:\Source\overlays\overlay-1.mov" --overlay-transparency 0.2 --title "红楼梦 第十三章" --subtitle "秦可卿死封龙禁尉 王熙凤协理宁国府"
    """
    args = parse_arguments()


    try:
        creator = VideoCreator(
            input_filelist=args.input_filelist,
            input_dir=args.input_dir if args.input_dir else r"I:\Source\封面\红楼梦\video",
            output_file=args.output_file if args.output_file else r"I:\Source\封面\红楼梦\video\output.mp4",
            overlay_video=args.overlay_file,
            overlay_transparency=args.overlay_transparency,
            duration=args.duration,
            title=args.title,
            subtitle=args.subtitle,
            transition_duration=args.transition_duration,
            use_gpu=args.use_gpu if args.use_gpu else True,
            log_level=args.log_level
        )
        creator.create_video()
    except Exception as e:
        logger.error("Failed to create video: %s", str(e))
        exit(1)

if __name__ == "__main__":
    main()