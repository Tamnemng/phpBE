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
    public string Id { get; set; }
    public int Quantity { get; set; }

    public Option()
    {
        Label = string.Empty;
        Id = string.Empty;
        Quantity = 0;
        Selected = false;
    }
    public Option(string label, string id, int quantity, bool selected = false)
    {
        Label = label;
        Id = id;
        Quantity = quantity;
        Selected = selected;
    }
}
