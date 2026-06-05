using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoiThatCaoCap.Models;
// Thay thế các namespace bên dưới cho đúng cấu trúc thư mục thực tế của bạn
using NoiThatCaoCap.Repositories;

namespace NoiThatCaoCap.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository; // Dùng để load danh mục không gian trưng bày
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Tiêm (Inject) các Repository và môi trường hệ thống vào Controller
        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. GET: Admin/Product
        public async Task<IActionResult> Index()
        {
            // Sử dụng Repository để lấy danh sách sản phẩm kèm theo Category tương ứng
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // 2. GET: Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách danh mục từ Category Repository để hiển thị lên dropdown list
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        // 3. POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? mainImage, List<IFormFile> detailImages)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                // --- Xử lý 1: Lưu file Ảnh đại diện (ImageUrl) ---
                if (mainImage != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = @"/images/products/" + fileName;
                }

                // Gọi Repository để thêm mới sản phẩm vào cơ sở dữ liệu
                await _productRepository.AddAsync(product);

                // --- Xử lý 2: Lưu Album ảnh chi tiết (Bảng ProductImage) ---
                if (detailImages != null && detailImages.Count > 0)
                {
                    foreach (var file in detailImages)
                    {
                        if (file.Length > 0)
                        {
                            string detailFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string detailPath = Path.Combine(wwwRootPath, @"images\products\details");

                            if (!Directory.Exists(detailPath))
                            {
                                Directory.CreateDirectory(detailPath);
                            }

                            using (var fileStream = new FileStream(Path.Combine(detailPath, detailFileName), FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            ProductImage productImage = new ProductImage
                            {
                                Url = @"/images/products/details/" + detailFileName,
                                ProductId = product.Id // product.Id lúc này đã tự sinh ra sau lệnh thêm sản phẩm ở trên
                            };

                            // Gọi hàm thêm ảnh chi tiết thông qua Repository
                            await _productRepository.AddAsync(product);
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu không hợp lệ, load lại danh sách danh mục
            var categoriesFallback = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryId = new SelectList(categoriesFallback, "Id", "Name", product.CategoryId);
            return View(product);
        }
    }
}