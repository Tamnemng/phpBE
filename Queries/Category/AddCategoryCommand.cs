using MediatR;

public class AddCategoryCommand : IRequest<Unit>
{

    public string Code { get; set; }
    public string Name { get; set; }
    public string CreatedBy { get; set; }

    public AddCategoryCommand(string code, string name, string createdBy)
    {
        Code = code;
        Name = name;
        CreatedBy = createdBy;
    }
}