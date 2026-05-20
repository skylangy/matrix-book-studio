

class Playlist:
    def __init__(self, playlist_id, title, description="", published_at="", channel_id="", channel_title="", thumbnails=None, default_language="", localized=None, privacy_status="", item_count=0, embed_html="", localizations=None, videos=None):
        self.playlist_id = playlist_id
        self.title = title
        self.description = description
        self.published_at = published_at
        self.channel_id = channel_id
        self.channel_title = channel_title
        self.thumbnails = thumbnails if thumbnails else {}
        self.default_language = default_language
        self.localized = localized if localized else {}
        self.privacy_status = privacy_status
        self.item_count = item_count
        self.embed_html = embed_html
        self.localizations = localizations if localizations else {}
        self.videos = videos if videos is not None else []

    def add_video(self, video):
        self.videos.append(video)

    def __str__(self):
        return f"Playlist(ID: {self.playlist_id}, Title: {self.title}, Description: {self.description}, PublishedAt: {self.published_at}, PrivacyStatus: {self.privacy_status}, ItemCount: {self.item_count})"
