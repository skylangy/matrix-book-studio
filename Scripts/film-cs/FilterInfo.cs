using System.Collections.Generic;

namespace FilmCS
{
    public class FilterInfo
    {
        public int InputIndex { get; set; } = 0;
        public string InputLabel { get; set; } = null;
        public string OutputLabel { get; set; } = null;
        public List<string> Filters { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"Index={InputIndex}, input_label={InputLabel}, output_label={OutputLabel}, filters=[{string.Join(",", Filters)}]";
        }
    }
}
