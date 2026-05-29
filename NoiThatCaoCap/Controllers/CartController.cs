using Microsoft.AspNetCore.Mvc;
using NoiThatCaoCap.Models;
using NoiThatCaoCap.Repositories;
using NoiThatCaoCap.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        // 1. Trang hiển thị danh sách giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return View(cart);
        }

        // 2. Chức năng thêm vào giỏ hàng (Đã fix đồng bộ tham số 'id' với View)
        [HttpGet]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cleanProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                Material = product.Material,
                Origin = product.Origin
                // Không bê thuộc tính Category vào đây để tránh bị lặp cấu trúc
            };

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.Product.Id == id);

            if (cartItem == null)
            {
                // Dùng bản sao cleanProduct đã được làm sạch để lưu vào giỏ hàng
                cart.Add(new CartItem { Product = cleanProduct, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }

        // 3. Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            cart.RemoveAll(item => item.Product.Id == id);
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }
        #endregion

        #region XỬ LÝ MỤC YÊU THÍCH (WISHLIST)
        // 1. Trang danh sách yêu thích
        public IActionResult Wishlist()
        {
            var wishlist = HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY) ?? new List<Product>();
            return View(wishlist);
        }

        // 2. Thêm vào danh sách yêu thích
        [HttpGet]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // SỬA LỖI CHO WISHLIST: Tạo bản sao sạch hoàn toàn trước khi đưa vào JSON Session
            var cleanProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                Material = product.Material,
                Origin = product.Origin
            };

            var wishlist = HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY) ?? new List<Product>();

            if (!wishlist.Any(p => p.Id == id))
            {
                wishlist.Add(cleanProduct);
            }

            HttpContext.Session.SetObjectAsJson(WISHLIST_KEY, wishlist);
            return RedirectToAction("Wishlist");
        }

        // 3. Xóa khỏi danh sách yêu thích
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