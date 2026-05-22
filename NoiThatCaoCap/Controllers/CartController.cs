using Microsoft.AspNetCore.Mvc;
using NoiThatCaoCap.Models;
using NoiThatCaoCap.Repositories;
using NoiThatCaoCap.Extensions;

namespace NoiThatCaoCap.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private const string CART_KEY = "PremiumCart";
        private const string WISHLIST_KEY = "PremiumWishlist";

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        #region XỬ LÝ GIỎ HÀNG (CART)
        // Hiển thị trang giỏ hàng công phu
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng từ nút trên Thẻ sản phẩm
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var product = _productRepository.GetAll()?.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.Product.Id == id);

            if (cartItem == null)
            {
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            cart.RemoveAll(item => item.Product.Id == id);
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }
        #endregion

        #region XỬ LÝ DANH SÁCH YÊU THÍCH (WISHLIST)
        // Hiển thị trang mục yêu thích
        public IActionResult Wishlist()
        {
            var wishlist = HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY) ?? new List<Product>();
            return View(wishlist);
        }

        // Thêm vào mục yêu thích (Nếu tồn tại rồi thì không thêm trùng)
        public IActionResult AddToWishlist(int id)
        {
            var product = _productRepository.GetAll()?.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var wishlist = HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY) ?? new List<Product>();

            if (!wishlist.Any(p => p.Id == id))
            {
                wishlist.Add(product);
            }

            HttpContext.Session.SetObjectAsJson(WISHLIST_KEY, wishlist);
            return RedirectToAction("Wishlist");
        }

        // Xóa sản phẩm khỏi danh sách yêu thích
        public IActionResult RemoveFromWishlist(int id)
        {
            var wishlist = HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY) ?? new List<Product>();
            wishlist.RemoveAll(p => p.Id == id);
            HttpContext.Session.SetObjectAsJson(WISHLIST_KEY, wishlist);
            return RedirectToAction("Wishlist");
        }
        #endregion
    }
}