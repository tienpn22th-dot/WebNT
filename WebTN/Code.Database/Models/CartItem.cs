using System;

namespace Code.Database.Models
{
    public class CartItem
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = string.Empty;
        public string Anh { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        // Tự động tính tổng tiền của từng món (Giá x Số lượng)
        public decimal ThanhTien => Gia * SoLuong;
    }
}