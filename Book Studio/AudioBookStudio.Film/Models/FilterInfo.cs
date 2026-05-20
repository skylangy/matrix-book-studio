namespace AudioBookStudio.Films.Models;
public class FilterInfo
{
    public int InputIndex { get; set; } = 0;
    public string InputLabel { get; set; } = string.Empty;
    public string OutputLabel { get; set; } = string.Empty;
    public FilterList Filters { get; set; } = [];

    public override string ToString()
    {
        return $"Index={InputIndex}, input_label={InputLabel}, output_label={OutputLabel}, filters=[{string.Join(",", Filters)}]";
    }
}