

class Video:
    def __init__(self, video_id, title, description="", published_at="", channel_id="", channel_title="", thumbnails=None, tags=None, category_id="22", live_broadcast_content="", default_language="", localized=None, default_audio_language="", duration="", dimension="", definition="", caption="", licensed_content=False, region_restriction=None, content_rating=None, projection="", has_custom_thumbnail=False, privacy_status="", upload_status="", failure_reason="", rejection_reason="", publish_at="", license="", embeddable=False, public_stats_viewable=False, made_for_kids=False, self_declared_made_for_kids=False, view_count="", like_count="", dislike_count="", favorite_count="", comment_count=""):
        self.video_id = video_id
        self.title = title
        self.description = description
        self.published_at = published_at
        self.channel_id = channel_id
        self.channel_title = channel_title
        self.thumbnails = thumbnails if thumbnails else {}
        self.tags = tags if tags else []
        self.category_id = category_id
        self.live_broadcast_content = live_broadcast_content
        self.default_language = default_language
        self.localized = localized if localized else {}
        self.default_audio_language = default_audio_language
        self.duration = duration
        self.dimension = dimension
        self.definition = definition
        self.caption = caption
        self.licensed_content = licensed_content
        self.region_restriction = region_restriction if region_restriction else {}
        self.content_rating = content_rating if content_rating else {}
        self.projection = projection
        self.has_custom_thumbnail = has_custom_thumbnail
        self.privacy_status = privacy_status
        self.upload_status = upload_status
        self.failure_reason = failure_reason
        self.rejection_reason = rejection_reason
        self.publish_at = publish_at
        self.license = license
        self.embeddable = embeddable
        self.public_stats_viewable = public_stats_viewable
        self.made_for_kids = made_for_kids
        self.self_declared_made_for_kids = self_declared_made_for_kids
        self.view_count = view_count
        self.like_count = like_count
        self.dislike_count = dislike_count
        self.favorite_count = favorite_count
        self.comment_count = comment_count

    def __str__(self):
        return f"Video(ID: {self.video_id}, Title: {self.title}, Description: {self.description}, PublishedAt: {self.published_at}, PrivacyStatus: {self.privacy_status})"
