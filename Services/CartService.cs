using bike.Models;
using Newtonsoft.Json;

namespace bike.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(CartItem item);
        void RemoveFromCart(int maXe, DateTime ngayNhanXe, DateTime ngayTraXe);
        void UpdateCustomerInfo(string hoTen, string soDienThoai, string email, string? ghiChuChung = null);
        void ClearCart();
        int GetCartItemCount();
    }

    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string CartSessionKey = "BikeCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new Cart();

            var cartJson = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }

            try
            {
                return JsonConvert.DeserializeObject<Cart>(cartJson) ?? new Cart();
            }
            catch
            {
                return new Cart();
            }
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();
            cart.AddItem(item);
            SaveCart(cart);
        }

        public void RemoveFromCart(int maXe, DateTime ngayNhanXe, DateTime ngayTraXe)
        {
            var cart = GetCart();
            cart.RemoveItem(maXe, ngayNhanXe, ngayTraXe);
            SaveCart(cart);
        }

        public void UpdateCustomerInfo(string hoTen, string soDienThoai, string email, string? ghiChuChung = null)
        {
            var cart = GetCart();
            cart.HoTen = hoTen;
            cart.SoDienThoai = soDienThoai;
            cart.Email = email;
            cart.GhiChuChung = ghiChuChung;
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(CartSessionKey);
        }

        public int GetCartItemCount()
        {
            return GetCart().TongSoXe;
        }

        private void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var cartJson = JsonConvert.SerializeObject(cart);
                session.SetString(CartSessionKey, cartJson);
            }
        }
    }
} 