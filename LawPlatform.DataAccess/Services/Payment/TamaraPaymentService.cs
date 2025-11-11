using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Payment;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LawPlatform.DataAccess.Services.Payment
{
    public class TamaraPaymentService : ITamaraPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TamaraPaymentService> _logger;
        private readonly ResponseHandler _responseHandler;
        private readonly LawPlatformContext _context;

        public TamaraPaymentService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TamaraPaymentService> logger,
            ResponseHandler responseHandler,
            LawPlatformContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _responseHandler = responseHandler;
            _context = context;

            // Configure HttpClient
            var baseUrl = _configuration["tamaraPayment:baseUrl"];
            var apiToken = _configuration["tamaraPayment:apiToken"];

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<TamaraCheckoutResponse>> CreateConsultationCheckoutAsync(ConsultationPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Tamara checkout for consultation: {ConsultationId}", request.ConsultationId);

                // Get consultation details
                var consultation = await _context.consultations
                    .Include(c => c.Client)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.ConsultationId);

                if (consultation == null)
                {
                    _logger.LogWarning("Consultation not found: {ConsultationId}", request.ConsultationId);
                    return _responseHandler.NotFound<TamaraCheckoutResponse>("Consultation not found.");
                }

                if (consultation.Status != ConsultationStatus.Active)
                {
                    _logger.LogWarning("Consultation is not active: {ConsultationId}", request.ConsultationId);
                    return _responseHandler.BadRequest<TamaraCheckoutResponse>("Consultation is not available for payment.");
                }

                // Validate and set payment type
                var paymentType = string.IsNullOrWhiteSpace(request.PaymentType)
                    ? "PAY_BY_INSTALMENTS"
                    : request.PaymentType;

                // Extract instalments from payment type if specified
                int? instalments = null;
                if (paymentType == "PAY_BY_INSTALMENTS")
                {
                    instalments = 3; // Default to 3 instalments for PAY_BY_INSTALMENTS
                }

                _logger.LogInformation("Using payment type: {PaymentType} with instalments: {Instalments} for consultation: {ConsultationId}",
                    paymentType, instalments, request.ConsultationId);

                // Default currency to SAR (Saudi Riyal)
                var currency = Currency.SAR.ToString();

                // Build Tamara checkout request with new structure
                var tamaraRequest = new TamaraCheckoutRequest
                {
                    order_reference_id = consultation.Id.ToString(),
                    order_number = $"CONSULT-{consultation.Id.ToString().Substring(0, 8)}",
                    total_amount = new MoneyAmount
                    {
                        amount = consultation.Budget,
                        currency = currency
                    },
                    shipping_amount = new MoneyAmount
                    {
                        amount = 0,
                        currency = currency
                    },
                    tax_amount = new MoneyAmount
                    {
                        amount = 0,
                        currency = currency
                    },
                    discount = new Discount
                    {
                        amount = new MoneyAmount
                        {
                            amount = 0,
                            currency = currency
                        },
                        name = "No Discount"
                    },
                    description = $"Legal Consultation: {consultation.Title}",
                    country_code = CountryCodes.SA.ToString(),
                    payment_type = paymentType,
                    instalments = instalments,
                    locale = "en_US",
                    platform = "LawPlatform",
                    is_mobile = false,
                    consumer = new Consumer
                    {
                        first_name = consultation.Client.FirstName,
                        last_name = consultation.Client.LastName,
                        phone_number = consultation.Client.User.PhoneNumber!,
                        email = consultation.Client.User.Email!
                    },
                    billing_address = new Address
                    {
                        first_name = consultation.Client.FirstName,
                        last_name = consultation.Client.LastName,
                        line1 = consultation.Client.Address ?? "N/A",
                        line2 = "",
                        city = consultation.Client.Address ?? "N/A",
                        region = consultation.Client.Address ?? "N/A",
                        country_code = CountryCodes.SA.ToString(),
                        phone_number = consultation.Client.User.PhoneNumber!
                    },
                    shipping_address = new Address
                    {
                        first_name = consultation.Client.FirstName,
                        last_name = consultation.Client.LastName,
                        line1 = consultation.Client.Address ?? "N/A",
                        line2 = "",
                        city = consultation.Client.Address ?? "N/A",
                        region = consultation.Client.Address ?? "N/A",
                        country_code = CountryCodes.SA.ToString(),
                        phone_number = consultation.Client.User.PhoneNumber!
                    },
                    items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            name = consultation.Title,
                            type = "Digital",
                            reference_id = consultation.Id.ToString(),
                            sku = $"CONSULT-{consultation.Specialization}",
                            quantity = 1,
                            discount_amount = new MoneyAmount
                            {
                                amount = 0,
                                currency = currency
                            },
                            tax_amount = new MoneyAmount
                            {
                                amount = 0,
                                currency = currency
                            },
                            unit_price = new MoneyAmount
                            {
                                amount = consultation.Budget,
                                currency = currency
                            },
                            total_amount = new MoneyAmount
                            {
                                amount = consultation.Budget,
                                currency = currency
                            }
                        }
                    },
                    merchant_url = new MerchantUrl
                    {
                        success = request.SuccessUrl ?? $"{_configuration["AppBaseUrl"]}/payment/success",
                        failure = request.FailureUrl ?? $"{_configuration["AppBaseUrl"]}/payment/failure",
                        cancel = request.CancelUrl ?? $"{_configuration["AppBaseUrl"]}/payment/cancel",
                        notification = $"{_configuration["AppBaseUrl"]}/api/payment/tamara/webhook"
                    }
                };

                // Send request to Tamara
                var createCheckoutPath = _configuration["tamaraPayment:paths:createCheckout"];
                var jsonContent = JsonSerializer.Serialize(tamaraRequest, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");


                _logger.LogInformation("Sending checkout request to Tamara. Request body: {RequestBody}", jsonContent);
                var response = await _httpClient.PostAsync(createCheckoutPath, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    var checkoutResponse = JsonSerializer.Deserialize<TamaraCheckoutResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Tamara checkout created successfully. CheckoutId: {CheckoutId}, OrderId: {OrderId}",
                        checkoutResponse.checkout_id, checkoutResponse.order_id);

                    // TODO: Store payment info in database (create a Payment entity if needed)

                    return _responseHandler.Success(checkoutResponse, "Checkout session created successfully. Redirect user to checkout URL.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<TamaraErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogError("Tamara checkout failed. Error: {Error}, Message: {Message}",
                        errorResponse?.error, errorResponse?.message);

                    return _responseHandler.BadRequest<TamaraCheckoutResponse>(
                        errorResponse?.message ?? "Failed to create checkout session with Tamara.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating Tamara checkout for consultation: {ConsultationId}", request.ConsultationId);
                return _responseHandler.ServerError<TamaraCheckoutResponse>("An error occurred while processing payment.");
            }
        }

        public async Task<Response<TamaraOrderResponse>> GetOrderDetailsAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Fetching order details from Tamara for orderId: {OrderId}", orderId);

                var getOrderPath = _configuration["tamaraPayment:paths:getOrderDetails"].Replace("{orderId}", orderId);
                var response = await _httpClient.GetAsync(getOrderPath);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var orderResponse = JsonSerializer.Deserialize<TamaraOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return _responseHandler.Success(orderResponse, "Order details retrieved successfully.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<TamaraErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogError("Failed to get order details. Error: {Error}", errorResponse?.message);
                    return _responseHandler.BadRequest<TamaraOrderResponse>(errorResponse?.message ?? "Failed to retrieve order details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching order details for orderId: {OrderId}", orderId);
                return _responseHandler.ServerError<TamaraOrderResponse>("An error occurred while fetching order details.");
            }
        }

        public async Task<Response<TamaraAuthorizeOrderResponse>> AuthorizeOrderAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Authorizing Tamara order: {OrderId}", orderId);

                var authorizePath = _configuration["tamaraPayment:paths:authoriseOrder"].Replace("{orderId}", orderId);
                var response = await _httpClient.PostAsync(authorizePath, null);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Tamara authorize order response: Status={StatusCode}, Body={ResponseBody}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var authorizeResponse = JsonSerializer.Deserialize<TamaraAuthorizeOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Order authorized successfully: {OrderId}, Status: {Status}, CaptureId: {CaptureId}", 
                        orderId, authorizeResponse.status, authorizeResponse.capture_id);

                    // Update consultation status to InProgress based on order_reference_id
                    // Get order details to find the consultation
                    var orderDetailsResponse = await GetOrderDetailsAsync(orderId);
                    if (orderDetailsResponse.Succeeded && orderDetailsResponse.Data != null)
                    {
                        if (Guid.TryParse(orderDetailsResponse.Data.order_reference_id, out var consultationId))
                        {
                            var consultation = await _context.consultations.FirstOrDefaultAsync(c => c.Id == consultationId);
                            if (consultation != null)
                            {
                                consultation.Status = ConsultationStatus.InProgress;
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Consultation {ConsultationId} marked as InProgress after authorization", consultationId);
                            }
                        }
                    }

                    return _responseHandler.Success(authorizeResponse, "Order authorized successfully.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<TamaraErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogError("Failed to authorize order: {OrderId}. Error: {Error}, Message: {Message}", 
                        orderId, errorResponse?.error, errorResponse?.message);
                    
                    return _responseHandler.BadRequest<TamaraAuthorizeOrderResponse>(
                        errorResponse?.message ?? "Failed to authorize order.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while authorizing order: {OrderId}", orderId);
                return _responseHandler.ServerError<TamaraAuthorizeOrderResponse>("An error occurred while authorizing order.");
            }
        }

        public async Task<Response<TamaraCancelOrderResponse>> CancelOrderAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Canceling Tamara order: {OrderId}", orderId);

                // First, get the order details to build the cancel request
                var orderDetailsResponse = await GetOrderDetailsAsync(orderId);
                if (!orderDetailsResponse.Succeeded || orderDetailsResponse.Data == null)
                {
                    _logger.LogError("Failed to retrieve order details for cancellation: {OrderId}", orderId);
                    return _responseHandler.BadRequest<TamaraCancelOrderResponse>("Failed to retrieve order details for cancellation.");
                }

                var orderDetails = orderDetailsResponse.Data;

                // Build cancel request from order details
                var cancelRequest = new TamaraCancelOrderRequest
                {
                    total_amount = orderDetails.total_amount,
                    shipping_amount = orderDetails.shipping_amount,
                    tax_amount = orderDetails.tax_amount,
                    discount_amount = orderDetails.discount_amount?.amount ?? new MoneyAmount
                    {
                        amount = 0,
                        currency = orderDetails.total_amount.currency
                    },
                    items = orderDetails.items?.Select(item => new CancelOrderItem
                    {
                        name = item.name,
                        type = item.type,
                        reference_id = item.reference_id,
                        sku = item.sku,
                        quantity = item.quantity,
                        discount_amount = item.discount_amount,
                        tax_amount = item.tax_amount,
                        unit_price = item.unit_price,
                        total_amount = item.total_amount
                    }).ToList() ?? new List<CancelOrderItem>()
                };

                var cancelPath = _configuration["tamaraPayment:paths:cancelOrder"].Replace("{orderId}", orderId);

                var jsonContent = JsonSerializer.Serialize(cancelRequest, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending cancel request to Tamara. Request body: {RequestBody}", jsonContent);

                var response = await _httpClient.PostAsync(cancelPath, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Tamara cancel order response: Status={StatusCode}, Body={ResponseBody}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var cancelResponse = JsonSerializer.Deserialize<TamaraCancelOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Order canceled successfully: {OrderId}, CancelId: {CancelId}", orderId, cancelResponse.cancel_id);

                    return _responseHandler.Success(cancelResponse, "Order canceled successfully.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<TamaraErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogError("Failed to cancel order: {OrderId}. Error: {Error}, Message: {Message}",
                        orderId, errorResponse?.error, errorResponse?.message);

                    return _responseHandler.BadRequest<TamaraCancelOrderResponse>(
                        errorResponse?.message ?? "Failed to cancel order.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while canceling order: {OrderId}", orderId);
                return _responseHandler.ServerError<TamaraCancelOrderResponse>("An error occurred while canceling order.");
            }
        }

        public async Task<Response<object>> GetPaymentOptionsAsync(PaymentOptionsRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching payment options for country: {Country}, amount: {Amount}, currency: {Currency}",
                    request.Country, request.Amount, request.Currency);

                var paymentTypesPath = _configuration["tamaraPayment:paths:getPaymentType"];

                // Build query string parameters for GET request according to Tamara API specification
                var orderValue = new
                {
                    amount = request.Amount.ToString("F2"),
                    currency = request.Currency.ToString()
                };

                var orderValueJson = JsonSerializer.Serialize(orderValue);

                var queryParams = new List<string>
                {
                    $"country={request.Country}",
                    $"order_value={Uri.EscapeDataString(orderValueJson)}"
                };

                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    queryParams.Add($"phone_number={Uri.EscapeDataString(request.PhoneNumber)}");
                }

                if (request.IsVip)
                {
                    queryParams.Add($"is_vip={request.IsVip.ToString().ToLower()}");
                }

                var queryString = string.Join("&", queryParams);
                var fullPath = $"{paymentTypesPath}?{queryString}";

                // Use GET request as specified by Tamara API
                var response = await _httpClient.GetAsync(fullPath);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    // First, try to deserialize as dynamic to see the structure
                    var jsonDocument = JsonDocument.Parse(responseContent);
                    _logger.LogInformation("JSON Structure: {Json}", jsonDocument.RootElement.ToString());

                    // Parse Tamara's payment options response
                    var serializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var paymentOptions = JsonSerializer.Deserialize<object>(responseContent, serializerOptions);

                    if (paymentOptions is null)
                    {
                        _logger.LogError("Failed to parse payment options response: {Response}", responseContent);
                        return _responseHandler.ServerError<object>("Failed to parse payment options.");
                    }


                    return _responseHandler.Success(paymentOptions, "Payment options retrieved successfully.");

                }
                else
                {
                    _logger.LogError("Failed to fetch payment options. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);
                    return _responseHandler.BadRequest<object>(
                        $"Failed to fetch payment options. Status: {response.StatusCode}, Response: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching payment options");
                return _responseHandler.InternalServerError<object>("An error occurred while fetching payment options.");
            }
        }

        public async Task<Response<bool>> ProcessWebhookNotificationAsync(TamaraWebhookNotification notification)
        {
            try
            {
                _logger.LogInformation("Processing webhook notification for order: {OrderId}, status: {Status}",
                    notification.order_id, notification.order_status);

                // Find the consultation by order reference ID
                if (Guid.TryParse(notification.order_reference_id, out var consultationId))
                {
                    var consultation = await _context.consultations.FirstOrDefaultAsync(c => c.Id == consultationId);

                    if (consultation != null)
                    {
                        // Update consultation status based on payment status
                        switch (notification.order_status?.ToLower())
                        {
                            case "approved":
                            case "authorized":
                                consultation.Status = ConsultationStatus.InProgress;
                                _logger.LogInformation("Consultation {ConsultationId} marked as InProgress after payment approval", consultationId);
                                break;

                            case "canceled":
                            case "declined":
                                consultation.Status = ConsultationStatus.Active;
                                _logger.LogInformation("Consultation {ConsultationId} reverted to Active after payment cancellation", consultationId);
                                break;
                        }

                        await _context.SaveChangesAsync();
                        return _responseHandler.Success(true, "Webhook processed successfully.");
                    }
                    else
                    {
                        _logger.LogWarning("Consultation not found for order reference: {OrderReferenceId}", notification.order_reference_id);
                        return _responseHandler.NotFound<bool>("Consultation not found.");
                    }
                }

                _logger.LogWarning("Invalid order reference ID: {OrderReferenceId}", notification.order_reference_id);
                return _responseHandler.BadRequest<bool>("Invalid order reference ID.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing webhook notification");
                return _responseHandler.ServerError<bool>("An error occurred while processing webhook.");
            }
        }
    }
}
