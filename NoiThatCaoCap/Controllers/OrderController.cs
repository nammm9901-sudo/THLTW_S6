using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoiThatCaoCap.Extensions;
using NoiThatCaoCap.Models;

namespace NoiThatCaoCap.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CART_KEY = "PremiumCart";

        public OrderController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── GET /Order/Checkout ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            var user = await _userManager.GetUserAsync(User);

            var vm = new CheckoutViewModel
            {
                ReceiverName    = user?.FullName ?? string.Empty,
                ReceiverPhone   = user?.PhoneNumber ?? string.Empty,
                ShippingAddress = user?.Address ?? string.Empty,
                CartItems       = cart,
                TotalPrice      = cart.Sum(i => i.Product.Price * i.Quantity)
            };

            return View(vm);
        }

        // ─── POST /Order/Checkout ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            vm.CartItems   = cart;
            vm.TotalPrice  = cart.Sum(i => i.Product.Price * i.Quantity);

            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId          = user!.Id,
                ReceiverName    = vm.ReceiverName,
                ReceiverPhone   = vm.ReceiverPhone,
                ShippingAddress = vm.ShippingAddress,
                Note            = vm.Note,
                TotalPrice      = vm.TotalPrice,
                OrderDate       = DateTime.Now,
                Status          = OrderStatus.Pending
            };

            foreach (var item in cart)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId       = item.Product.Id,
                    ProductName     = item.Product.Name,
                    ProductImageUrl = item.Product.ImageUrl,
                    UnitPrice       = item.Product.Price,
                    Quantity        = item.Quantity
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Xóa giỏ sau khi đặt hàng thành công
            HttpContext.Session.Remove(CART_KEY);

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        // ─── GET /Order/Success/5 ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Chỉ cho xem đơn của chính mình
            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Forbid();

            return View(order);
        }

        // ─── GET /Order/Profile ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _db.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Orders = orders;
            return View(user);
        }

        // ─── POST /Order/UpdateProfile ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string? phoneNumber, string? address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName     = fullName;
            user.PhoneNumber  = phoneNumber;
            user.Address      = address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                TempData["ProfileSuccess"] = "Cập nhật hồ sơ thành công!";
            else
                TempData["ProfileError"] = "Có lỗi xảy ra, vui lòng thử lại.";

            return RedirectToAction(nameof(Profile));
        }

        // ─── Helper ───────────────────────────────────────────────────
        private List<CartItem> GetCart()
            => HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
    }
}
