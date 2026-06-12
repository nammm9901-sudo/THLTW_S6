using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoiThatCaoCap.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string ReceiverName { get; set; } = string.Empty;

        [Required]
        public string ReceiverPhone { get; set; } = string.Empty;

        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderDetail> OrderDetails { get; set; } = new();
    }

    public enum OrderStatus
    {
        Pending = 0,       // Chờ xác nhận
        Confirmed = 1,     // Đã xác nhận
        Shipping = 2,      // Đang giao
        Delivered = 3,     // Đã giao
        Cancelled = 4      // Đã hủy
    }
}
