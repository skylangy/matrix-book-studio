import os
import pickle
import json
from datetime import datetime, timedelta

from googleapiclient.discovery import build
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from googleapiclient.http import MediaFileUpload

from video import Video
from playlist import Playlist
from booksorter import BookSorter
from logger import Logger

SCOPES = ["https://www.googleapis.com/auth/youtube.upload",
          "https://www.googleapis.com/auth/youtube.force-ssl",
          "https://www.googleapis.com/auth/youtubepartner",
          "'https://www.googleapis.com/auth/youtubepartner-content-owner'"]

class YouTubeManager:
    def __init__(self):
        self.logger = Logger(self.__class__.__name__)
        self.service = self.get_authenticated_service()

    def get_authenticated_service(self):
        creds = None
        if os.path.exists("token.pickle"):
            with open("token.pickle", "rb") as token:
                creds = pickle.load(token)
        if not creds or not creds.valid:
            if creds and creds.expired and creds.refresh_token:
                creds.refresh(Request())
            else:
                flow = InstalledAppFlow.from_client_secrets_file("client_secret.json", SCOPES)
                creds = flow.run_local_server(port=0)
            with open("token.pickle", "wb") as token:
                pickle.dump(creds, token)
        return build("youtube", "v3", credentials=creds)

    def get_recent_playlists(self) -> list:
        request = self.service.playlists().list(
            part="snippet,contentDetails,status",
            mine=True,
            maxResults=50
        )
        response = request.execute()
        playlists = []
        for item in response.get("items", []):
            playlist = Playlist(
                playlist_id=item["id"],
                title=item["snippet"]["title"],
                description=item["snippet"].get("description", ""),
                published_at=item["snippet"].get("publishedAt", ""),
                channel_id=item["snippet"].get("channelId", ""),
                channel_title=item["snippet"].get("channelTitle", ""),
                thumbnails=item["snippet"].get("thumbnails", {}),
                default_language=item["snippet"].get("defaultLanguage", ""),
                localized=item["snippet"].get("localized", {}),
                privacy_status=item["status"].get("privacyStatus", ""),
                item_count=item["contentDetails"].get("itemCount", 0),
                embed_html=item.get("player", {}).get("embedHtml", ""),
                localizations=item.get("localizations", {})
            )
            playlists.append(playlist)
        return sorted(playlists, key=lambda x: x.published_at, reverse=True)

    def get_playlist_by_id(self, playlist_id):
        request = self.service.playlists().list(
            part="snippet,contentDetails,status",
            id=playlist_id
        )
        response = request.execute()
        if "items" in response and len(response["items"]) > 0:
            item = response["items"][0]
            playlist = Playlist(
                playlist_id=item["id"],
                title=item["snippet"]["title"],
                description=item["snippet"].get("description", ""),
                published_at=item["snippet"].get("publishedAt", ""),
                channel_id=item["snippet"].get("channelId", ""),
                channel_title=item["snippet"].get("channelTitle", ""),
                thumbnails=item["snippet"].get("thumbnails", {}),
                default_language=item["snippet"].get("defaultLanguage", ""),
                localized=item["snippet"].get("localized", {}),
                privacy_status=item["status"].get("privacyStatus", ""),
                item_count=item["contentDetails"].get("itemCount", 0),
                embed_html=item.get("player", {}).get("embedHtml", ""),
                localizations=item.get("localizations", {})
            )
            return playlist
        else:
            self.logger.info(f"No playlist found with ID: {playlist_id}")
            return None

    def get_videos_in_playlist(self, playlist_id):
        videos = []
        request = self.service.playlistItems().list(
            part="snippet",
            playlistId=playlist_id,
            maxResults=150  # Adjust as needed
        )
        response = request.execute()
        for item in response.get("items", []):
            snippet = item["snippet"]
            video_id = snippet["resourceId"]["videoId"]
            title = snippet["title"]
            description = snippet.get("description", "")
            published_at = snippet["publishedAt"]
            thumbnails = snippet["thumbnails"]

            video_obj = Video(video_id, title, description, published_at, thumbnails)
            videos.append(video_obj)

        videos = BookSorter().sort_videos(videos)
        return videos

    def get_video_by_id(self, video_id):
        request = self.service.videos().list(
            part="snippet,contentDetails,statistics,status",
            id=video_id
        )
        response = request.execute()
        if "items" in response and len(response["items"]) > 0:
            item = response["items"][0]
            video = Video(
                video_id=item["id"],
                title=item["snippet"]["title"],
                description=item["snippet"].get("description", ""),
                published_at=item["snippet"].get("publishedAt", ""),
                channel_id=item["snippet"].get("channelId", ""),
                channel_title=item["snippet"].get("channelTitle", ""),
                thumbnails=item["snippet"].get("thumbnails", {}),
                tags=item["snippet"].get("tags", []),
                category_id=item["snippet"].get("categoryId", "22"),
                live_broadcast_content=item["snippet"].get("liveBroadcastContent", ""),
                default_language=item["snippet"].get("defaultLanguage", ""),
                localized=item["snippet"].get("localized", {}),
                default_audio_language=item["snippet"].get("defaultAudioLanguage", ""),
                duration=item["contentDetails"].get("duration", ""),
                dimension=item["contentDetails"].get("dimension", ""),
                definition=item["contentDetails"].get("definition", ""),
                caption=item["contentDetails"].get("caption", ""),
                licensed_content=item["contentDetails"].get("licensedContent", False),
                region_restriction=item["contentDetails"].get("regionRestriction", {}),
                content_rating=item["contentDetails"].get("contentRating", {}),
                projection=item["contentDetails"].get("projection", ""),
                has_custom_thumbnail=item["contentDetails"].get("hasCustomThumbnail", False),
                privacy_status=item["status"].get("privacyStatus", ""),
                upload_status=item["status"].get("uploadStatus", ""),
                failure_reason=item["status"].get("failureReason", ""),
                rejection_reason=item["status"].get("rejectionReason", ""),
                publish_at=item["status"].get("publishAt", ""),
                license=item["status"].get("license", ""),
                embeddable=item["status"].get("embeddable", False),
                public_stats_viewable=item["status"].get("publicStatsViewable", False),
                made_for_kids=item["status"].get("madeForKids", False),
                self_declared_made_for_kids=item["status"].get("selfDeclaredMadeForKids", False),
                view_count=item["statistics"].get("viewCount", ""),
                like_count=item["statistics"].get("likeCount", ""),
                dislike_count=item["statistics"].get("dislikeCount", ""),
                favorite_count=item["statistics"].get("favoriteCount", ""),
                comment_count=item["statistics"].get("commentCount", "")
            )
            return video
        else:
            self.logger.info(f"No video found with ID: {video_id}")
            return None

    def update_video_status(self, video: Video, status: str, publish_at: str):
        self.logger.info(f"Update video status: {video.video_id}, from: {video.privacy_status}, {video.publish_at}, to: {status}, {publish_at}")
        body = {
            "id": video.video_id,
            "status": {
                "privacyStatus": status,
                "publishAt": publish_at
            },
            'snippet': {
                'title': video.title,
                'categoryId': video.category_id
            },
            'sponsorsOnly':{
                'isSponsorsOnly': True
            }
        }
        request = self.service.videos().update(
            part='snippet,status',
            body=body
        )
        response = request.execute()
        return response

    def update_video_property(self, video: Video, tags:list = None, description: str = None):
        body = {
            "id": video.video_id,
            "status": {
                "selfDeclaredMadeForKids": False
            }
        }

        if tags is not None:
            body["snippet"] = {
                'title': video.title,
                'categoryId': video.category_id,
                "tags": tags,
                'description': description if description is not None else video.description,
                }

        part = "status"
        if tags is not None or description is not None:
            part += ",snippet"

        request = self.service.videos().update(
            part=part,
            body=body
        )
        response = request.execute()
        return response

    def update_video_properties(
        self,
        video: Video,
        publish_at: str = None,
        status: str = 'private',
        tags: list = None,
        description: str = None
    ):
        # Initialize the request body with the video ID
        self.logger.info(f"Updating video: {video.video_id} with status: {status} and publishAt: {publish_at}, {video.publish_at}, tags: {tags}, description: {description}")
        # Initialize the request body with the video ID
        body = {
            "id": video.video_id,
            "status": {
                "selfDeclaredMadeForKids": False
            },
            "snippet": {
                'title': video.title,
                'categoryId': video.category_id
            }
        }

        # Update status properties if provided
        if status is not None:
            body["status"]["privacyStatus"] = status
        if publish_at is not None:
            body["status"]["publishAt"] = publish_at
            body["snippet"]["scheduledStartTime"] = publish_at

        # Update tags and description if provided
        if tags is not None or description is not None:
            body["snippet"]["tags"] = tags if tags is not None else video.tags
            body["snippet"]["description"] = description if description is not None else video.description

        # Determine the parts to be updated
        parts = ["status", "snippet"]
        part = ",".join(parts)

        self.logger.info(f'Update body: {body}')
        # Execute the update request
        request = self.service.videos().update(
            part=part,
            body=body
        )
        response = request.execute()
        self.logger.info(f"Updated video: {response}")
        return response

    def publish_playlist_daily(self, playlist_id, start_date: datetime, count_per_day: int, status='private', dry_run=False):
        self.logger.info(f"Publishing videos in playlist {playlist_id} starting from {start_date} with {count_per_day} videos per day.")
        playlist = self.get_playlist_by_id(playlist_id)
        if not playlist:
            self.logger.info(f"Playlist with ID {playlist_id} not found.")
            return

        videos = self.get_videos_in_playlist(playlist_id)
        if not videos:
            self.logger.info(f"No videos found in playlist {playlist_id}.")
            return

        if status is None:
            status = 'private'
        publish_date = start_date
        videos_published_today = 0
        videos_published_total = 0

        for video in videos:
            if videos_published_total >= len(videos):
                break

            if videos_published_today >= count_per_day:
                publish_date += timedelta(days=1)
                videos_published_today = 0

            if dry_run:
                self.logger.info(f"Would publish on {publish_date} video: {video.title}, {status}")
                videos_published_today += 1
                videos_published_total += 1
            else:
                # Update video publishAt field
                publish_at = publish_date.strftime('%Y-%m-%dT%H:%M:%SZ')  # Convert to ISO 8601 format
                try:
                    self.logger.info(f"Publishing video: {video.title} on {publish_at}, {status}")
                    self.update_video_status(video, status, publish_at)
                    self.logger.info(f"Published video: on {publish_at} video: {video.title} ")
                except Exception as e:
                    self.logger.info(f"Failed to publish video: {video.title}. \n\tError: {e}")
                finally:
                    videos_published_today += 1
                    videos_published_total += 1
                    self.logger.info(f"\n")

        if dry_run:
            self.logger.info(f"Dry run completed. No videos were actually published.")
        else:
            self.logger.info(f"Publishing completed for playlist {playlist_id}.")

    def create_playlist(self, title, description, privacy_status='public'):
        request = self.service.playlists().insert(
            part="snippet,status",
            body={
                "snippet": {
                    "title": title,
                    "description": description
                },
                "status": {
                    "privacyStatus": privacy_status
                }
            }
        )
        response = request.execute()
        return response

    def upload_video_to_playlist(self, playlist_id, file_path, title, description, tags, privacy_status="private"):
        self.logger.info(f"Uploading video: {title}, to playlist: {playlist_id}, {file_path}")
        media = MediaFileUpload(file_path, chunksize=-1, resumable=True)
        request = self.service.videos().insert(
            part="snippet,status",
            body={
                "snippet": {
                    "categoryId": "22",  # Replace with actual category ID if needed
                    "title": title,
                    "description": description,
                    "tags": tags,
                    "playlistId": playlist_id  # Associate video with playlist
                },
                "status": {
                    "privacyStatus": privacy_status,
                    "selfDeclaredMadeForKids": False,
                    "license": "youtube"
                }
            },
            media_body=media
        )

        try:
            response = request.execute()
            self.logger.info(f"Uploaded video: {response['id']} - {response['snippet']['title']}")
            return response
        except Exception as e:
            self.logger.info(f"Error uploading video: {e}")
            return None

