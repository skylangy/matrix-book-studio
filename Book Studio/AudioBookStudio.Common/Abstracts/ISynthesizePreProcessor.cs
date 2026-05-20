namespace AudioBookStudio.Common.Abstracts;
public interface ISynthesizePreProcessor
{
    Task<string> ProcessAsync(string content);
}
