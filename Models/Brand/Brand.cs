using OMS.Core.Utilities;

public class Brand
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }

    public Brand()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Image = string.Empty;
    }

    public Brand(string name, string code, string image)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Image = image;
    }

    public Brand(AddBrandCommand command)
    {
        Id = IdGenerator.GenerateId(16);
        Code = command.Code;
        Name = command.Name;
        Image = command.Image;
    }

    public static Brand Create(string code, string name, string image)
    {
        return new Brand
        {
            Code = code,
            Name = name,
            Image = image,
        };
    }

    public void Update(UpdateBrandCommand command)
    {
        Name = command.Name;
        Image = command.Image;
    }
}
public class BrandMetaData : BaseEntity
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }

    public BrandMetaData() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Image = string.Empty;
    }

    // Constructor with creation details
    public BrandMetaData(string createdBy, string code, string name, string image)
        : base(createdBy)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Image = image;
    }

    public void Update(UpdateBrandCommand brandCommand, string updatedBy)
    {
        // Preserve the original creation information
        base.Update(updatedBy);
        
        // Update only mutable properties
        Name = brandCommand.Name;
        Image = brandCommand.Image;
    }
}