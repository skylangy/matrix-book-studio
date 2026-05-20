namespace AudioBookStudio.Models.Data;
public enum PropertyType
{
    Text,
    Resource
}

public class TemplateProperty
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Tag { get; set; }
    public PropertyType Type { get; set; }
}