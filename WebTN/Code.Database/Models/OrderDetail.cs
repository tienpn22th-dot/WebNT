using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.Models // Thay namespace phù hợp với dự án của ông
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("OrderId")]
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; } // Giá sản phẩm tại thời điểm mua
    }
}