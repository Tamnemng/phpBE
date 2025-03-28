using MediatR;

public class AddBrandCommand : IRequest<Unit>
{

    public string Code { get; set; }
    public string Name { get; set; }
    public string Logo { get; set; }
    public string CreatedBy { get; set; }

    public AddBrandCommand(string code, string name, string logo, string createdBy)
    {
        Code = code;
        Name = name;
        Logo= logo;
        CreatedBy = createdBy;
    }
}