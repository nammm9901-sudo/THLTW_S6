using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoiThatCaoCap.Areas.Admin.Models;
using NoiThatCaoCap.Models;
using NoiThatCaoCap.Repositories;
using System.IO;

namespace NoiThatCaoCap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        // 1. INDEX - Hiển thị danh sách sản phẩm (kèm Category để hiển thị tên danh mục)
        public async Task<IActionResult> Index()
        {
            // Đảm bảo Repository của bạn đã bao gồm dữ liệu Category (Include) bên trong GetAllAsync()
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // 2. GET: CREATE
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        // 3. POST: CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            IFormFile? mainImage,
            List<IFormFile> detailImages)
        {
            // Loại bỏ kiểm tra điều kiện tự động cho các thuộc tính liên kết ảo để tránh lỗi ModelState không đáng có
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                string wwwRoot = _webHostEnvironment.WebRootPath;

                // Xử lý lưu ảnh đại diện (Main Image) trước
                if (mainImage != null && mainImage.Length > 0)
                {
                    string folder = Path.Combine(wwwRoot, "images", "products");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    await using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fs);
                    }
                    product.ImageUrl = "/images/products/" + fileName;
                }

                // Lưu sản phẩm vào Database trước để Entity Framework sinh ra product.Id tự động
                await _productRepository.AddAsync(product);

                // Xử lý lưu bộ sưu tập ảnh chi tiết (Detail Images nếu có)
                if (detailImages != null && detailImages.Count > 0)
                {
                    string detailFolder = Path.Combine(wwwRoot, "images", "products", "details");
                    if (!Directory.Exists(detailFolder)) Directory.CreateDirectory(detailFolder);

                    product.Images ??= new List<ProductImage>();

                    foreach (var file in detailImages.Where(f => f.Length > 0))
                    {
                        string fn = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(detailFolder, fn);

                        await using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fs);
                        }

                        // Áp dụng chuẩn theo Model ProductImage (Id, Url, ProductId)
                        product.Images.Add(new ProductImage
                        {
                            Url = "/images/products/details/" + fn,
                            ProductId = product.Id
                        });
                    }
                    // Cập nhật lại sản phẩm sau khi đã gán danh sách ảnh chi tiết thành công
                    await _productRepository.UpdateAsync(product);
                }

                TempData["Success"] = "Thêm sản phẩm nghệ thuật thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu không hợp lệ, trả lại form và nạp lại dropdown list
            var cats = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryId = new SelectList(cats, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 4. GET: EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            if (p == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", p.CategoryId);
            return View(p);
        }

        // 5. POST: EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? mainImage)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                var existing = await _productRepository.GetByIdAsync(product.Id);
                if (existing == null) return NotFound();

                // Cập nhật các thông tin cơ bản từ Form gửi lên
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.CategoryId = product.CategoryId;
                existing.Material = product.Material;
                existing.Origin = product.Origin;
                existing.Description = product.Description;

                // Nếu Admin tải lên ảnh mới, tiến hành thay thế ảnh cũ
                if (mainImage != null && mainImage.Length > 0)
                {
                    string wwwRoot = _webHostEnvironment.WebRootPath;
                    string folder = Path.Combine(wwwRoot, "images", "products");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    // Xóa file ảnh đại diện cũ trên ổ đĩa để giải phóng dung lượng (nếu có ảnh cũ)
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwRoot, existing.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu file ảnh mới
                    string fn = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string filePath = Path.Combine(folder, fn);

                    await using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fs);
                    }
                    existing.ImageUrl = "/images/products/" + fn;
                }

                await _productRepository.UpdateAsync(existing);
                TempData["Success"] = "Cập nhật tác phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 6. GET: DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _productRepository.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // 7. POST: DELETE (Xác nhận xóa hẳn khỏi hệ thống)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                string wwwRoot = _webHostEnvironment.WebRootPath;

                // 1. Dọn dẹp ảnh đại diện cũ trên ổ đĩa server
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string mainImagePath = Path.Combine(wwwRoot, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(mainImagePath)) System.IO.File.Delete(mainImagePath);
                }

                // 2. Dọn dẹp toàn bộ ảnh chi tiết trong thư mục vật lý (nếu có nạp kèm danh sách Images)
                if (product.Images != null && product.Images.Any())
                {
                    foreach (var img in product.Images)
                    {
                        string detailPath = Path.Combine(wwwRoot, img.Url.TrimStart('/'));
                        if (System.IO.File.Exists(detailPath)) System.IO.File.Delete(detailPath);
                    }
                }

                // 3. Xóa bản ghi trong Database thông qua Repository
                await _productRepository.DeleteAsync(id);
                TempData["Success"] = "Đã gỡ bỏ tác phẩm khỏi bộ sưu tập!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}