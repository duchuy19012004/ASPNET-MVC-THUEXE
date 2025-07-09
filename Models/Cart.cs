using System.ComponentModel.DataAnnotations;

namespace bike.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Customer information
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Ghi chú chung")]
        [StringLength(1000)]
        public string? GhiChuChung { get; set; }

        // Calculated properties
        public int TongSoXe => Items.Count;
        public decimal TongTien => Items.Sum(item => item.TongTien);
        
        // Helper methods
        public void AddItem(CartItem item)
        {
            // Check if same bike with same dates already exists
            var existingItem = Items.FirstOrDefault(i => 
                i.MaXe == item.MaXe && 
                i.NgayNhanXe == item.NgayNhanXe && 
                i.NgayTraXe == item.NgayTraXe);

            if (existingItem == null)
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int maXe, DateTime ngayNhanXe, DateTime ngayTraXe)
        {
            var item = Items.FirstOrDefault(i => 
                i.MaXe == maXe && 
                i.NgayNhanXe == ngayNhanXe && 
                i.NgayTraXe == ngayTraXe);
            
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
} 