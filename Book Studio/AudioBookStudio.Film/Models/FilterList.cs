namespace AudioBookStudio.Films.Models;
public class FilterList : List<string>
{
    public FilterList()
    {
    }

    public FilterList(IEnumerable<string> filters)
        : base(filters)
    {
    }

    public FilterList Clean()
    {
        var cleaned = new FilterList(
         this.Where(item => !string.IsNullOrWhiteSpace(item)));

        return cleaned;
    }

    public string Join(string separator)
    {
        return string.Join(separator, Clean());
    }
}

