
from film import Film
from filmSettings import FilmSettings
from models import Margin, TextModel, Size, Align

class FilmBuilder:

    @staticmethod
    def build_film_with_intro_outro(
        intro_path: str,
        main_video_path: str,
        outro_path: str,
        audio_path: str,
        intro_duration: float = 3.0,
        outro_duration: float = 3.0,
        settings: FilmSettings = FilmSettings()
    ) -> Film:

        film = Film(settings)
        film.add_image(
            path=intro_path,
            start=0.0,
            duration=intro_duration,
        ).add_repeat_video_track(
            video_path=main_video_path,
            audio_path=audio_path,
            start=intro_duration,
        ).add_image(
            path=outro_path,
            duration=outro_duration,
        ).add_audio_track(
            path=audio_path,
            start=intro_duration,
        )
        return film

    @staticmethod
    def build_film_with_images_logo(
        images_folder: str,
        overlay_video_path: str,
        logo_image_path: str,
        name_texts: list[TextModel] = None,
        logo_texts: list[TextModel] = None,
        images_duration: float = 4.5,
        image_transition: str = 'fade',
        image_transition_duration: float = 0.5,
        overlay_video_alpha: float = 0.2,
        name_texts_margin: Margin = Margin(left=50, top=60, right=0, bottom=0),
        name_texts_align: Align = Align.CENTER,
        logo_image_size: Size = Size(64, 64),
        log_image_margin: Margin = Margin(left=0, top=50, right=50, bottom=0),
        logo_image_align: Align = Align.TOP_RIGHT,
        logo_image_alpha: float = 1.0,
        logo_text_margin: Margin = Margin(left=0, right=30, top=120, bottom=0),
        logo_text_align: Align = Align.TOP_RIGHT,
        settings: FilmSettings = FilmSettings()
    ):
        film = Film(settings)
        film.add_images_from_folder(
            folder=images_folder,
            image_duration=images_duration,
            transition=image_transition,
            transition_duration=image_transition_duration
        ).add_video_overlay(
            path=overlay_video_path,
            alpha=overlay_video_alpha
        ).add_text_group(
            texts=name_texts,
            margin=name_texts_margin,
            align=name_texts_align,
            line_spacing=15,
        ).add_image_overlay(
            path=logo_image_path,
            size=logo_image_size,
            margin=log_image_margin,
            align=logo_image_align,
            alpha=logo_image_alpha
        ).add_text_group(
            texts=logo_texts,
            margin=logo_text_margin,
            line_spacing=15,
            align=logo_text_align
        )
        return film