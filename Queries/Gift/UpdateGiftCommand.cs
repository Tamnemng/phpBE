using MediatR;

public class UpdateGiftCommand : IRequest<Unit>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateGiftCommand(string code, string name, string image, bool isSellable, string updatedBy)
    {
        Code = code;
        Name = name;
        Image = image;
        UpdatedBy = updatedBy;
    }
}