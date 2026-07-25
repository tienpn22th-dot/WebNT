using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Database.Models
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }
    }
}