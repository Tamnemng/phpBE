using MediatR;

public class AddBrandCommand : IRequest<Unit>
{

    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string CreatedBy { get; set; }

    public AddBrandCommand(string code, string name, string image, string createdBy)
    {
        Code = code;
        Name = name;
        Image = image;
        CreatedBy = createdBy;
    }
}