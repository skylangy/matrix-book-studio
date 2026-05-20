from youtube import YouTubeManager
from bookloader import BookLoader
from datetime import datetime
from argparse import ArgumentParser

def print_separator():
    print(f"\n{'='*50}\n")

def print_properties(obj, indent=4):
        padding = '  ' * indent
        if isinstance(obj, dict):
            for key, value in obj.items():
                if isinstance(value, (dict, list)):
                    print(f"{padding}{key}:")
                    print_properties(value, indent + 1)
                else:
                    print(f"{padding}{key}: {value}")
        elif isinstance(obj, list):
            for index, item in enumerate(obj):
                print(f"{padding}[{index}]:")
                print_properties(item, indent + 1)

def get_playlists(youtube: YouTubeManager):
    playlists = youtube.get_recent_playlists()

    for playlist in playlists:
        print(f"ID: {playlist.playlist_id}, Title: {playlist.title}, Count: {playlist.item_count}, Video: {len(playlist.videos)}, Published At: {playlist.published_at}")

def main():
    # py .\publish.py --func publish --playlistId "PLXqYrMN7koreH5pSwN1ZZU6Kg5YZ57BLC" --startDate "2024-11-01T06:00:00.0Z" --countPerDay 3
    # py .\publish.py --func prepare --playlistId "PLOa8nrfQY_--a83jw1XWjspxnkS5r-SB1" --tags "毛泽东,中国,静听书屋,冯客" --description "毛泽东之后的中国"
    # py .\publish.py --func list --playlistId "PLOa8nrfQY_--a83jw1XWjspxnkS5r-SB1"
    # py .\publish.py --func status --playlistId "PLOa8nrfQY_--Y1E8Q-9wHYtCrpEbl68Tp" --status "private"
    parser = ArgumentParser(description="Publish YouTube playlist videos daily.")

    # Add arguments
    parser.add_argument('--func', type=str, help='The func to run.')
    parser.add_argument('--playlistId', type=str, help='The ID of the YouTube playlist.')
    parser.add_argument('--startDate', type=lambda d: datetime.strptime(d, '%Y-%m-%dT%H:%M:%S.%fZ'), help='The start date for publishing videos (format: YYYY-MM-DDTHH:MM:SS.sssZ).')
    parser.add_argument('--countPerDay', type=int, help='The number of videos to publish per day.')
    parser.add_argument('--tags', type=str, help='Tags for the YouTube video.')
    parser.add_argument('--description', type=str, help='Description of the video or playlist.')
    parser.add_argument('--status', type=str, help='Status of video.')
    # parser.add_argument('--dry_run', type=bool, help='Dry run the publish process.')

    # Parse the arguments
    args = parser.parse_args()

    youtube = YouTubeManager()
    if args.func == "publish":
        youtube.publish_playlist_daily(args.playlistId, args.startDate, args.countPerDay, args.status, dry_run=False)
    elif args.func == "prepare":
        tags = args.tags.split(',') if args.tags else None
        print(tags)
        videos = youtube.get_videos_in_playlist(args.playlistId)
        index = 0
        for video in videos:
            if index >=1:
                break
            youtube.update_video_properties(video, tags = tags, description=args.description)
            index += 1
    elif args.func == "list":
        videos = youtube.get_videos_in_playlist(args.playlistId)
        for video in videos:
            print(f"ID: {video.video_id}, Title: {video.title}, Published At: {video.published_at}, Privacy Status: {video.privacy_status}")
    elif args.func == "status":
        videos = youtube.get_videos_in_playlist(args.playlistId)
        for video in videos:
            print(f"Update ID: {video.video_id}, Title: {video.title}, Published At: {video.published_at}, Privacy Status: {video.privacy_status}")
            youtube.update_video_status(video, args.status, None)

if __name__ == "__main__":
    main()
