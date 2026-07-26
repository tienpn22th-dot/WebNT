using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Database.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public int MaDh { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DiaChi { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = "Chờ xác nhận";

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}