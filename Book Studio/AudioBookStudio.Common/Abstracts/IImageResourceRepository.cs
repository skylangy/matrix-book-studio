namespace AudioBookStudio.Common.Abstracts;

[Obsolete("This interface is obsolete. Use IImageResourceRepository instead.")]
public interface IImageResourceRepository
{
    Task<ImageResource> AddAsync(ImageResource book);
    Task<ImageResource> GetByIdAsync(string id);
    Task<bool> UpdateAsync(ImageResource book);
}
