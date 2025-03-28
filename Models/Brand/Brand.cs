using OMS.Core.Utilities;

public class Brand
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Logo { get; set; }

    public Brand()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Logo = string.Empty;
    }

    public Brand(string name, string code, string logo)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Logo = logo;
    }

    public Brand(AddBrandCommand command)
    {
        Id = IdGenerator.GenerateId(16);
        Code = command.Code;
        Name = command.Name;
        Logo = command.Logo;
    }

    public static Brand Create(string code, string name, string logo)
    {
        return new Brand
        {
            Code = code,
            Name = name,
            Logo = logo,
        };
    }

    public void Update(UpdateBrandCommand command)
    {
        Name = command.Name;
        Logo = command.Logo;
    }
}
public class BrandMetaData : BaseEntity
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Logo { get; set; }

    public BrandMetaData() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
        Logo = string.Empty;
    }

    // Constructor with creation details
    public BrandMetaData(string createdBy, string code, string name, string logo)
        : base(createdBy)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Code = code;
        Logo = logo;
    }

    public void Update(UpdateBrandCommand brandCommand, string updatedBy)
    {
        // Preserve the original creation information
        base.Update(updatedBy);
        
        // Update only mutable properties
        Name = brandCommand.Name;
        Logo = brandCommand.Logo;
    }
}