using E_Cart.Domain.Common;
using E_Cart.Domain.Enums;


namespace ECart.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }

        // Foreign Key (one-to-one with Order)
        public int OrderId { get; set; }

        // Navigation property
        public Order Order { get; set; } = null!;
    }
}