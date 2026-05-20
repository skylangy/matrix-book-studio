namespace AudioBookStudio.Films.Models;
public class ImageBlocks
{
    private readonly IList<ImageBlock> _blocks = [];

    public ImageBlocks()
    {
    }

    public ImageBlocks(IEnumerable<ImageBlock> blocks)
    {
        foreach (var block in blocks)
        {
            AddBlock(block);
        }
    }

    public IReadOnlyList<ImageBlock> Blocks => _blocks.AsReadOnly();

    public ImageBlocks AddBlock(ImageBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        _blocks.Add(block);
        return this;
    }
}
