namespace AudioBookStudio.Common.Abstracts;
public interface IVideoExportService
{
    Task Export(VideoMeta videoMeta);
    Task ExportVideoOnly(VideoMeta videoMeta);
}
