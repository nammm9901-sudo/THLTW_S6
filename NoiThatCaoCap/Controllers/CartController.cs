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

        // ─────────────────────────────────────────────
        //  GIỎ HÀNG / ĐƠN HÀNG
        // ─────────────────────────────────────────────

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Thêm vào giỏ — hỗ trợ cả GET (từ nút nhanh) và POST
        [HttpGet]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.Product.Id == id);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = CleanProduct(product),
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            // Nếu request là AJAX / fetch thì trả JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = cart.Sum(i => i.Quantity) });

            TempData["CartMsg"] = "Đã thêm vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng từ trang giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.Product.Id == id);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // Xóa 1 sản phẩm khỏi giỏ
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.Product.Id == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────
        //  WISHLIST
        // ─────────────────────────────────────────────

        // Hiển thị danh sách yêu thích
        public IActionResult Wishlist()
        {
            var wishlist = GetWishlist();
            return View(wishlist);
        }

        // Toggle: thêm nếu chưa có, xóa nếu đã có
        [HttpGet]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var wishlist = GetWishlist();
            var existed = wishlist.Any(p => p.Id == id);

            if (!existed)
                wishlist.Add(CleanProduct(product));

            SaveWishlist(wishlist);

            // Hỗ trợ AJAX (gọi bằng fetch từ JS)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, added = !existed, count = wishlist.Count });

            TempData["WishMsg"] = existed ? "Sản phẩm đã có trong danh sách yêu thích."
                                          : "Đã thêm vào danh sách yêu thích!";
            return RedirectToAction("Wishlist");
        }

        // Chuyển từ wishlist sang giỏ hàng
        [HttpGet]
        public async Task<IActionResult> MoveToCart(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Thêm vào giỏ
            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.Product.Id == id);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem { Product = CleanProduct(product), Quantity = 1 });
            SaveCart(cart);

            // Xóa khỏi wishlist
            var wishlist = GetWishlist();
            wishlist.RemoveAll(p => p.Id == id);
            SaveWishlist(wishlist);

            TempData["CartMsg"] = "Đã chuyển sản phẩm vào giỏ hàng!";
            return RedirectToAction("Wishlist");
        }

        // Xóa khỏi wishlist
        public IActionResult RemoveFromWishlist(int id)
        {
            var wishlist = GetWishlist();
            wishlist.RemoveAll(p => p.Id == id);
            SaveWishlist(wishlist);
            return RedirectToAction("Wishlist");
        }

        // ─────────────────────────────────────────────
        //  HELPERS DÙNG CHUNG
        // ─────────────────────────────────────────────

        private List<CartItem> GetCart()
            => HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
               ?? new List<CartItem>();

        private void SaveCart(List<CartItem> cart)
            => HttpContext.Session.SetObjectAsJson(CART_KEY, cart);

        private List<Product> GetWishlist()
            => HttpContext.Session.GetObjectFromJson<List<Product>>(WISHLIST_KEY)
               ?? new List<Product>();

        private void SaveWishlist(List<Product> wishlist)
            => HttpContext.Session.SetObjectAsJson(WISHLIST_KEY, wishlist);

        /// <summary>
        /// Tạo bản sao Product sạch (không có navigation property) để tránh lỗi JSON circular reference trong Session.
        /// </summary>
        private static Product CleanProduct(Product p) => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Description = p.Description,
            Material = p.Material,
            Origin = p.Origin,
            CategoryId = p.CategoryId
        };
    }
}