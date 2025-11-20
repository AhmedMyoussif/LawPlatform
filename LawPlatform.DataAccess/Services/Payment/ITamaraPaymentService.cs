using LawPlatform.Entities.DTO.Payment;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Payment
{
    public interface ITamaraPaymentService
    {
        Task<Response<TamaraCheckoutResponse>> CreateConsultationCheckoutAsync(ConsultationPaymentRequest request);
        Task<Response<TamaraOrderResponse>> GetOrderDetailsAsync(string orderId);
        Task<Response<TamaraAuthorizeOrderResponse>> AuthorizeOrderAsync(string orderId);
        Task<Response<TamaraCancelOrderResponse>> CancelOrderAsync(string orderId);
        Task<Response<object>> GetPaymentOptionsAsync(PaymentOptionsRequest request);
        Task<Response<bool>> ProcessWebhookNotificationAsync(TamaraWebhookNotification notification);
    }
}
