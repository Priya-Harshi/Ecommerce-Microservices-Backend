using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Repositories;

namespace PaymentService.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;

    public PaymentService(
        IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(
        CreatePaymentDto dto)
    {
        if (dto.OrderId <= 0)
        {
            throw new ArgumentException(
                "Invalid OrderId.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "Payment amount must be greater than zero.");
        }

        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Status = "Paid",
            CreatedAt = DateTime.UtcNow
        };

        var created =
            await _repository.CreateAsync(payment);

        return MapToDto(created);
    }

    public async Task<PaymentDto?> GetByIdAsync(
        int id)
    {
        var payment =
            await _repository.GetByIdAsync(id);

        return payment == null
            ? null
            : MapToDto(payment);
    }

    private static PaymentDto MapToDto(
        Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status,
            PaymentMethod = payment.PaymentMethod,
            CreatedAt = payment.CreatedAt
        };
    }
}