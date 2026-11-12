using PaymentService.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentDto> ProcessPaymentAsync(
        CreatePaymentDto dto);

    Task<PaymentDto?> GetByIdAsync(int id);
}