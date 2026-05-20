using MatrixBook.Server.Models;

namespace MatrixBook.Server.Services;

public interface IOptionsService
{
    Task<OptionCollection> GetOptionsAsync();

    Task<OptionCollection> UpdateOptionsAsync(OptionCollection options);
}
