using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Data;

namespace MatrixBook.Server.Services;

public interface IBookExportService
{
    void ExportBook(ExportModel model);

    Task EnhanceMp3(Book book);

    Task OpenBookFolder(string bookFolder);

    Task SelectFile(string fileName);
}
