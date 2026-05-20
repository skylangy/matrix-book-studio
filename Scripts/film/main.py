
from film import Film
from models import Margin, TextModel, Size, Align
from filmBuilder import FilmBuilder
from filmSettings import FilmSettings

def generate_short_chapter():
    film = Film()
    film.add_images_from_folder(
        folder=r'I:\Source\封面\红楼梦\video',
        image_duration=4.5,
        transition='fade',
        transition_duration=0.5
    ).add_video_overlay(
        path=r'I:\Source\overlays\overlay-5.mp4',
        alpha=0.2
    ).add_text_group(
        texts=[
           TextModel('红楼梦 第一回', fontsize=28, show_shadow=True),
           TextModel('甄士隐梦幻识通灵 贾雨村风尘怀闺秀', fontsize=20, show_shadow=True),
        ],
        margin=Margin(
            left=50,
            top=60,
            right=0,
            bottom=0
        ),
        align=Align.CENTER,
        line_spacing=15,
    ).add_image_overlay(
        path=r'I:\Source\overlays\book-logo.png',
        size=Size(64, 64),
        margin=Margin(
            left=0,
            top=50,
            right=50,
            bottom=0
        ),
        align=Align.TOP_RIGHT,
        alpha=1.0
    ).add_text_group(
        texts=[
           TextModel('深夜书屋', fontsize=24, show_shadow=True)
        ],
        margin=Margin(
            left=0,
            right=30,
            top=120,
            bottom=0
        ),
        line_spacing=15,
        align=Align.TOP_RIGHT
    )

    film.run(output_path=r'I:\Source\封面\红楼梦\video\第一章-short.mp4')

def generate_full_chapter():
    film = Film()
    film.add_image(
        path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        start=0.0,
        duration=3,
    ).add_repeat_video_track(
        video_path=r'I:\Source\封面\红楼梦\video\第一章-short.mp4',
        audio_path=r'I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3',
        start=3.0,
    ).add_image(
        path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        duration=3,
    ).add_audio_track(
        path=r'I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3',
        start=3.0,
    )
    film.run(output_path=r'I:\Source\封面\红楼梦\video\第一章-full.mp4')

def generate_full():
    film = Film()
    film.add_image(
        path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        start=0.0,
        duration=3,
    ).add_video_track(
        path=r'I:\Source\封面\红楼梦\video\第一章-short.mp4',
        duration=1800
    ).add_image(
        path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        start=0.0,
        duration=3,
    ).add_audio_track(
        path=r'I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3',
        start=3.0,
    )
    film.run(output_path=r'I:\Source\封面\红楼梦\video\第一章-full.mp4')

def build_repeat_video():
    settings = FilmSettings(output_path=r'I:\Source\封面\红楼梦\video\第一章-repeat.mp4')
    film = FilmBuilder.build_film_with_images_logo(
       images_folder=r'I:\Source\封面\红楼梦\video',
       overlay_video_path=r'I:\Source\overlays\overlay-5.mp4',
       logo_image_path=r'I:\Source\overlays\book-logo.png',
       name_texts=[
           TextModel('红楼梦 第一回', fontsize=28, show_shadow=True),
           TextModel('甄士隐梦幻识通灵 贾雨村风尘怀闺秀', fontsize=20, show_shadow=True),
       ],
       logo_texts=[
           TextModel('深夜书屋', fontsize=24, show_shadow=True)
       ],
       settings=settings
    )
    film.run()

def build_film_with_intro_outro():
    settings = FilmSettings(output_path=r'I:\Source\封面\红楼梦\video\第一章-full.mp4')
    film = FilmBuilder.build_film_with_intro_outro(
        intro_path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        main_video_path=r'I:\Source\封面\红楼梦\video\第一章-short.mp4',
        outro_path=r'I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg',
        audio_path=r'I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3',
        settings=settings
    )
    film.run()

def main():
    # generate_short_chapter()
    # generate_full_chapter()
    # generate_full()
    build_repeat_video()
    # build_film_with_intro_outro()

if __name__ == '__main__':
    main()