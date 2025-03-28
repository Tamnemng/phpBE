using System;
using System.Collections.Generic;
using Google.Rpc;
using OMS.Core.Utilities;

public class Category
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public Category()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
    }

    public Category(AddCategoryCommand command)
    {
        Id = IdGenerator.GenerateId(16);
        Code = command.Code;
        Name = command.Name;
    }

    public void Update(UpdateCategoryCommand command)
    {
        Name = command.Name;
    }
}

public class CategoryMetaData : BaseEntity
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public CategoryMetaData() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Code = string.Empty;
    }

    public CategoryMetaData(AddCategoryCommand command) : base(command.CreatedBy)
    {
        Id = IdGenerator.GenerateId(16);
        Code = command.Code;
        Name = command.Name;
    }

    public void Update(UpdateCategoryCommand command)
    {
        base.Update(command.UpdatedBy);
        Name = command.Name;
    }
}
