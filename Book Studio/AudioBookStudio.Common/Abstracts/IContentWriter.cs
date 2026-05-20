using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;


public interface IContentWriter
{
    Task<bool> WriteAsync(WriteContentContext context);
}
