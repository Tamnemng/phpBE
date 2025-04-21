// Controllers/PaymentController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("process-online-payment")]
    public async Task<IActionResult> ProcessOnlinePayment([FromBody] ProcessPaymentDto paymentDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            // First check if order exists and belongs to user
            var getOrderQuery = new GetOrderByIdQuery(paymentDto.OrderId, userId);
            var order = await _mediator.Send(getOrderQuery);
            
            // In a real application, you would integrate with a payment gateway here
            // For this example, we'll simulate a successful payment
            
            // Update payment status to Paid
            var updatePaymentCommand = new UpdatePaymentStatusCommand(
                paymentDto.OrderId, 
                PaymentStatus.Paid, 
                username
            );
            
            await _mediator.Send(updatePaymentCommand);
            
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thanh toán thành công!"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.Unauthorized, "UNAUTHORIZED"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PAYMENT_PROCESS_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}

// Payment DTO
