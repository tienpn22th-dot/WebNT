using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.Models // Thay namespace phù hợp với dự án của ông
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderCode { get; set; } = string.Empty; // Mã đơn hàng (VD: DH001)

        [Required]
        [MaxLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? CustomerAddress { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public double TotalAmount { get; set; }

        public int Status { get; set; } = 0; // 0: Mới / Chờ duyệt, 1: Đã duyệt / Đang giao, 2: Hoàn thành, 3: Hủy

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}