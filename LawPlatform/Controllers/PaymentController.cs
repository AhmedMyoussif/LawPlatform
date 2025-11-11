using LawPlatform.DataAccess.Services.Payment;
using LawPlatform.Entities.DTO.Payment;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    /// <summary>
    /// Controller for handling Tamara payment operations for consultations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ITamaraPaymentService _paymentService;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ITamaraPaymentService paymentService,
            ResponseHandler responseHandler,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        /// <summary>
        /// Creates a Tamara checkout session for a consultation payment
        /// </summary>
        /// <param name="request">Consultation payment request with consultation ID and payment details</param>
        /// <returns>Checkout URL to redirect user to Tamara payment page</returns>
        [HttpPost("checkout")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateConsultationCheckout([FromBody] ConsultationPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating consultation checkout for ConsultationId: {ConsultationId}", request.ConsultationId);

                var result = await _paymentService.CreateConsultationCheckoutAsync(request);

                if (result.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation checkout");
                return StatusCode(500, _responseHandler.ServerError<object>("An error occurred while creating checkout"));
            }
        }

        /// <summary>
        /// Gets available payment options for a specific amount and country
        /// </summary>
        /// <param name="request">Payment options request with country and amount</param>
        /// <returns>List of available payment plans</returns>
        [HttpPost("payment-options")]
        public async Task<IActionResult> GetPaymentOptions([FromBody] PaymentOptionsRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching payment options for country: {Country}, amount: {Amount}",
                    request.Country, request.Amount);

                var result = await _paymentService.GetPaymentOptionsAsync(request);

                if (result.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment options");
                return StatusCode(500, _responseHandler.ServerError<object>("An error occurred while fetching payment options"));
            }
        }

        /// <summary>
        /// Gets order details from Tamara
        /// </summary>
        /// <param name="orderId">Tamara order ID</param>
        /// <returns>Order details including status and amount</returns>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            try
            {
                _logger.LogInformation("Fetching order details for orderId: {OrderId}", orderId);

                var result = await _paymentService.GetOrderDetailsAsync(orderId);

                if (result.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order details");
                return StatusCode(500, _responseHandler.ServerError<object>("An error occurred while fetching order details"));
            }
        }

        /// <summary>
        /// Authorizes a Tamara order after customer completes payment
        /// </summary>
        /// <param name="orderId">Tamara order ID</param>
        /// <returns>Authorization result</returns>
        [HttpPost("authorize/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorizeOrder(string orderId)
        {
            try
            {
                _logger.LogInformation("Authorizing order: {OrderId}", orderId);

                var result = await _paymentService.AuthorizeOrderAsync(orderId);

                if (result.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing order");
                return StatusCode(500, _responseHandler.ServerError<object>("An error occurred while authorizing order"));
            }
        }

        /// <summary>
        /// Cancels a Tamara order
        /// </summary>
        /// <param name="orderId">Tamara order ID</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("cancel/{orderId}")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            try
            {
                _logger.LogInformation("Canceling order: {OrderId}", orderId);

                var result = await _paymentService.CancelOrderAsync(orderId);

                if (result.Succeeded)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order");
                return StatusCode(500, _responseHandler.ServerError<object>("An error occurred while canceling order"));
            }
        }

        /// <summary>
        /// Webhook endpoint to receive payment notifications from Tamara
        /// </summary>
        /// <param name="notification">Webhook notification from Tamara</param>
        /// <returns>Acknowledgment response</returns>
        //[HttpPost("webhook")]
        //[AllowAnonymous]
        //public async Task<IActionResult> TamaraWebhook([FromBody] TamaraWebhookNotification notification)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Received Tamara webhook notification for order: {OrderId}, status: {Status}",
        //            notification.order_id, notification.order_status);

        //        // TODO: Verify webhook signature using notificationPrivateKey from config

        //        var result = await _paymentService.ProcessWebhookNotificationAsync(notification);

        //        if (result.Succeeded)
        //        {
        //            return Ok(new { message = "Webhook processed successfully" });
        //        }

        //        return BadRequest(new { message = "Webhook processing failed" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing Tamara webhook");
        //        return StatusCode(500, new { message = "An error occurred while processing webhook" });
        //    }
        //}
    }
}
