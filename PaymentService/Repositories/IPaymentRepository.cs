using PaymentService.Models;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);

    Task<Payment?> GetByIdAsync(int id);
}