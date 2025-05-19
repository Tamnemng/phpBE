using System.ComponentModel.DataAnnotations;

public class OrderItemSelectionDto
{
    [Required]
    public string ItemId { get; set; }

    [Required]
    public CartItemType ItemType { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}