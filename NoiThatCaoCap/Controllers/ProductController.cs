using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
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
            return await _categoryRepository.GetAllAsync();
        }

        // GET: /Product/Index
        public async Task<IActionResult> Index(string search, List<int> categories, List<string> prices, List<string> materials)
        {
            var allProducts = await _productRepository.GetAllAsync();
            var products = allProducts.AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                            || (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                                            || (p.Material != null && p.Material.Contains(search, StringComparison.OrdinalIgnoreCase)));

            // Lọc theo danh mục
            if (categories != null && categories.Any())
                products = products.Where(p => categories.Contains(p.CategoryId));

            // Lọc theo mức giá (VNĐ, đơn vị triệu)
            if (prices != null && prices.Any())
            {
                products = products.Where(p =>
                    (prices.Contains("Under20") && p.Price < 20_000_000) ||
                    (prices.Contains("20to50") && p.Price >= 20_000_000 && p.Price < 50_000_000) ||
                    (prices.Contains("50to100") && p.Price >= 50_000_000 && p.Price < 100_000_000) ||
                    (prices.Contains("Above100") && p.Price >= 100_000_000)
                );
            }

            // Lọc theo chất liệu (so sánh linh hoạt, không phân biệt hoa thường)
            if (materials != null && materials.Any())
                products = products.Where(p =>
                    p.Material != null &&
                    materials.Any(m => p.Material.Contains(m, StringComparison.OrdinalIgnoreCase))
                );

            ViewBag.SelectedCategories = categories ?? new List<int>();
            ViewBag.SelectedPrices = prices ?? new List<string>();
            ViewBag.SelectedMaterials = materials ?? new List<string>();
            ViewBag.Categories = await GetCategoriesAsync();
            ViewBag.SearchTerm = search;

            return View(products.ToList());
        }

        // GET: /Product/Search?searchTerm=... (từ thanh tìm kiếm header)
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            return RedirectToAction(nameof(Index), new { search = searchTerm });
        }

        // GET: /Product/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var all = await _productRepository.GetAllAsync();
            ViewBag.RelatedProducts = all
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4).ToList();

            return View(product);
        }

        // GET: /Product/Add
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            ViewBag.CategoryId = new SelectList(await GetCategoriesAsync(), "Id", "Name");
            return View();
        }

        // POST: /Product/Add
        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        // GET: /Product/Update/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            ViewBag.CategoryId = new SelectList(await GetCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Product/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Product product, List<IFormFile> ImageFiles)
        {
            var existing = await _productRepository.GetByIdAsync(product.Id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.CategoryId = product.CategoryId;
            existing.Material = product.Material;
            existing.Origin = product.Origin;
            existing.Description = product.Description;

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

        // GET: /Product/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            return p == null ? NotFound() : View(p);
        }

        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}