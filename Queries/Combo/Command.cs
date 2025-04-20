using MediatR;
using System.Collections.Generic;
using OMS.Core.Queries;

// Add Combo Command
public class AddComboCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<string> ProductCodes { get; set; }
    public decimal ComboPrice { get; set; }
    public string CreatedBy { get; set; }

    public AddComboCommand(string name, string description, string imageUrl, List<string> productCodes, decimal comboPrice, string createdBy)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ProductCodes = productCodes;
        ComboPrice = comboPrice;
        CreatedBy = createdBy;
    }
}

// Update Combo Command
public class UpdateComboCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<string> ProductCodes { get; set; }
    public decimal? ComboPrice { get; set; }
    public bool? IsActive { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateComboCommand(string id, string name, string description, string imageUrl, 
                             List<string> productCodes, decimal? comboPrice, bool? isActive, string updatedBy)
    {
        Id = id;
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ProductCodes = productCodes;
        ComboPrice = comboPrice;
        IsActive = isActive;
        UpdatedBy = updatedBy;
    }
}

// Delete Combo Command
public class DeleteComboCommand : IRequest<bool>
{
    public string Id { get; set; }

    public DeleteComboCommand(string id)
    {
        Id = id;
    }
}

// Get All Combos Query
public class GetAllCombosQuery : IRequest<PagedModel<ComboDto>>
{
    public int PageIndex { get; }
    public int PageSize { get; }
    public bool IncludeInactive { get; }

    public GetAllCombosQuery(int pageIndex = 0, int pageSize = 10, bool includeInactive = false)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        IncludeInactive = includeInactive;
    }
}

// Get Combo By Id Query
public class GetComboByIdQuery : IRequest<ComboDto>
{
    public string Id { get; }

    public GetComboByIdQuery(string id)
    {
        Id = id;
    }
}