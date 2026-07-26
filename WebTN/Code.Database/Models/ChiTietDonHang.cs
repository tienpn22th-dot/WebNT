using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Database.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int MaCt { get; set; }

        public int MaDh { get; set; }
        [ForeignKey("MaDh")]
        public virtual DonHang? DonHang { get; set; }

        public int MaSp { get; set; }
        [ForeignKey("MaSp")]
        public virtual SanPham? SanPham { get; set; }

        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }
    }
}