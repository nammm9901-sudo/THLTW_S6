using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NoiThatCaoCap.Models;
using NoiThatCaoCap.Repositories;
using System;
using System.Linq;

namespace NoiThatCaoCap.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH SẢN PHẨM (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(
    string search,
    List<int> categories,
    List<string> prices,
    List<string> materials)
        {
            var products = _productRepository.GetAll().AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            // FILTER CATEGORY
            if (categories != null && categories.Any())
            {
                products = products.Where(p => categories.Contains(p.CategoryId));
            }

            // FILTER MATERIAL
            if (materials != null && materials.Any())
            {
                products = products.Where(p => materials.Contains(p.Material));
            }

            // FILTER PRICE
            if (prices != null && prices.Any())
            {
                var priceFiltered = products.Where(p => false);

                if (prices.Contains("Under20"))
                {
                    priceFiltered = priceFiltered.Union(
                        products.Where(p => p.Price < 20000000)
                    );
                }

                if (prices.Contains("20to50"))
                {
                    priceFiltered = priceFiltered.Union(
                        products.Where(p =>
                            p.Price >= 20000000 &&
                            p.Price <= 50000000)
                    );
                }

                if (prices.Contains("50to100"))
                {
                    priceFiltered = priceFiltered.Union(
                        products.Where(p =>
                            p.Price > 50000000 &&
                            p.Price <= 100000000)
                    );
                }

                if (prices.Contains("Above100"))
                {
                    priceFiltered = priceFiltered.Union(
                        products.Where(p => p.Price > 100000000)
                    );
                }

                products = priceFiltered;
            }

            // VIEWBAG
            ViewBag.Categories = _categoryRepository.GetAll();

            ViewBag.SelectedCategories = categories ?? new List<int>();

            ViewBag.SelectedPrices = prices ?? new List<string>();

            ViewBag.SelectedMaterials = materials ?? new List<string>();

            ViewBag.Search = search;

            return View(products.ToList());
        }

        // ==========================================
        // 2. CHI TIẾT SẢN PHẨM (DETAIL)
        // ==========================================
        public IActionResult Detail(int id)
        {
            var product = _productRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // ==========================================
        // 3. FORM THÊM MỚI SẢN PHẨM (GET)
        // ==========================================
        public IActionResult Add()
        {
            // Load danh mục để hiển thị lên thẻ <select> trong View
            var categories = _categoryRepository.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        // ==========================================
        // 4. XỬ LÝ THÊM MỚI SẢN PHẨM (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepository.Add(product);
                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu lỗi, load lại SelectList để người dùng chọn lại không bị crash
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================
        // 5. FORM CẬP NHẬT SẢN PHẨM (GET)
        // ==========================================
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            // Điền sẵn danh mục cũ của sản phẩm vào thẻ select
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================
        // 6. XỬ LÝ CẬP NHẬT SẢN PHẨM (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepository.Update(product);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // ==========================================
        // 7. XÁC NHẬN XÓA SẢN PHẨM (GET)
        // ==========================================
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // ==========================================
        // 8. XỬ LÝ XÓA SẢN PHẨM (POST)
        // ==========================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            _productRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Product/Search?searchTerm=Sofa
        public IActionResult Search(string searchTerm)
        {
            var products = _productRepository.GetAll() ?? new List<Product>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Chuyển từ khóa về chữ thường để tìm kiếm không phân biệt hoa thường
                string keyword = searchTerm.Trim().ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(keyword) ||
                                               p.Description.ToLower().Contains(keyword)).ToList();
            }

            // Truyền từ khóa ngược lại qua ViewBag để hiển thị trên thanh thông báo nếu muốn
            ViewBag.SearchTerm = searchTerm;

            // Sử dụng lại giao diện Index để hiển thị kết quả tìm kiếm cho đồng bộ thiết kế
            return View("Index", products);
        }
    }
}