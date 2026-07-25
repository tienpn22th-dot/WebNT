using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Database.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int MaSp { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không vượt quá 100 ký tự")]
        public string TenSp { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public string? Anh { get; set; }

        public string? MoTa { get; set; }

        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }
    }
}