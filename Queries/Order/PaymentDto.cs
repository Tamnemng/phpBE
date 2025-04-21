using System.ComponentModel.DataAnnotations;

public class ProcessPaymentDto
{
    [Required]
    public string OrderId { get; set; }
    
    // Other payment-related properties would go here in a real application
    // For example: payment method details, card information, etc.
}