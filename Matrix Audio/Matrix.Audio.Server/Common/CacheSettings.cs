namespace Matrix.Audio.Server.Common;

public class CacheSettings
{
    public const int ResponseCacheDurationLong = 120;
    public const int ResponseCacheDurationMid = 60;
    public const int ResponseCacheDurationShort = 30;

    public const int CacheDurationInMinuteMid = 15;
    public const int CacheDurationInMinuteShort = 5;
    public const int CacheDurationInMinuteLong = 30;

    public const string NotFoundKey = "__NOT_FOUND__";
}
