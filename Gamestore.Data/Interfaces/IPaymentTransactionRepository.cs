using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    Task<IEnumerable<PaymentTransaction>> GetTransactionsByOrderIdAsync(Guid orderId);
    Task<PaymentTransaction?> GetLatestTransactionByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PaymentTransaction>> GetTransactionsByCustomerAsync(Guid customerId);
}

