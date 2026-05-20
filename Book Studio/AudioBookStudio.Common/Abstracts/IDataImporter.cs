namespace AudioBookStudio.Common.Abstracts;
public interface IDataImporter
{
    Task Import<T>(IEnumerable<T> items) where T : class;
}
