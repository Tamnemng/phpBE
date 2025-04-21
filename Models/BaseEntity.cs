public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }

    public BaseEntity()
    {
        CreatedDate = DateTime.Today;
        CreatedBy = string.Empty;
    }

    public BaseEntity(string createdBy)
    {
        CreatedDate = DateTime.Today;
        CreatedBy = createdBy;
    }

    public virtual void Update(string updatedBy)
    {
        UpdatedDate = DateTime.Today;
        UpdatedBy = updatedBy;
    }
}