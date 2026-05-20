using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface IVideoCombiner
{
    Task<VideoCombineResult> Combine(VideoCombineContext context);
}
