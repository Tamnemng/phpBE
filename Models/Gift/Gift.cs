using OMS.Core.Utilities;

public class Gift
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public Gift()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Image = string.Empty;
    }

    public Gift(string name, string code, string image)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Image = image;
    }

    public Gift(AddGiftCommand command)
    {
        Id = IdGenerator.GenerateId(16);
        Code = command.Code;
        Name = command.Name;
        Image = command.Image;
    }

    public static Gift Create(string code, string name, string image)
    {
        return new Gift
        {
            Code = code,
            Name = name,
            Image = image
        };
    }

    public void Update(UpdateGiftCommand command)
    {
        Name = command.Name;
        Image = command.Image;
    }
}

public class GiftMetaData : BaseEntity
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }

    public GiftMetaData() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Image = string.Empty;
    }

    // Constructor with creation details
    public GiftMetaData(string createdBy, string code, string name, string image)
        : base(createdBy)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Image = image;
    }

    public void Update(UpdateGiftCommand giftCommand, string updatedBy)
    {
        base.Update(updatedBy);
        Name = giftCommand.Name;
        Image = giftCommand.Image;
    }
}