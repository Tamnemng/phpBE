using System.Collections.Generic;

public class ProductOption
{
    public string Title { get; set; }
    public List<Option> Options { get; set; }

    public ProductOption()
    {
        Options = new List<Option>();
        Title = string.Empty;
    }
    public ProductOption(string title, List<Option> options)
    {
        Title = title;
        Options = options;
    }
}
public class Option
{
    public string Label { get; set; }
    public bool Selected { get; set; }

    public Option()
    {
        Label = string.Empty;
        Selected = false;
    }
    public Option(string label, bool selected = false)
    {
        Label = label;
        Selected = selected;
    }
}
