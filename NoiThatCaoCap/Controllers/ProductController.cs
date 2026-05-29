using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoiThatCaoCap.Models;
using NoiThatCaoCap.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NoiThatCaoCap.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if (categories == null || !categories.Any())
            {
                var samples = new List<Category> {
                    new Category { Name = "Sofa Phòng Khách" },
                    new Category { Name = "Bàn Ghế Ăn" },
                    new Category { Name = "Giường Ngủ Cao Cấp" },
                    new Category { Name = "Tủ Kệ Trang Trí" }
                };
                foreach (var cat in samples) await _categoryRepository.AddAsync(cat);
                categories = await _categoryRepository.GetAllAsync();
            }
            return categories;
        }

        public async Task<IActionResult> Index(string search, List<int> categories, List<string> prices, List<string> materials)
        {
            var allProducts = await _productRepository.GetAllAsync();
            var products = allProducts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (categories != null && categories.Any())
                products = products.Where(p => categories.Contains(p.CategoryId));

            ViewBag.SelectedCategories = categories ?? new List<int>();
            ViewBag.SelectedPrices = prices ?? new List<string>();
            ViewBag.SelectedMaterials = materials ?? new List<string>();
            ViewBag.Categories = await GetCategoriesAsync();

            return View(products.ToList());
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var all = await _productRepository.GetAllAsync();
            ViewBag.RelatedProducts = all.Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id).Take(4).ToList();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ViewBag.CategoryId = new SelectList(await GetCategoriesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, List<IFormFile> ImageFiles)
        {
            if (ModelState.IsValid)
            {
                product.Images ??= new List<ProductImage>();

                if (ImageFiles != null)
                {
                    string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    Directory.CreateDirectory(folder);

                    foreach (var file in ImageFiles.Where(f => f.Length > 0))
                    {
                        string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        string url = "/images/products/" + fileName;
                        product.Images.Add(new ProductImage { Url = url });
                        if (string.IsNullOrEmpty(product.ImageUrl)) product.ImageUrl = url;
                    }
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(await GetCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            ViewBag.CategoryId = new SelectList(await GetCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Product product, List<IFormFile> ImageFiles)
        {
            // Lấy thực thể đang được track từ Database
            var existing = await _productRepository.GetByIdAsync(product.Id);
            if (existing == null) return NotFound();

            // Cập nhật các trường dữ liệu
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.CategoryId = product.CategoryId;
            existing.Material = product.Material;
            existing.Origin = product.Origin;
            existing.Description = product.Description;

            // Xử lý ảnh mới
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                existing.Images ??= new List<ProductImage>();

                foreach (var file in ImageFiles.Where(f => f.Length > 0))
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    string url = "/images/products/" + fileName;
                    existing.Images.Add(new ProductImage { Url = url, ProductId = existing.Id });
                    if (string.IsNullOrEmpty(existing.ImageUrl)) existing.ImageUrl = url;
                }
            }

            await _productRepository.UpdateAsync(existing);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            return p == null ? NotFound() : View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}