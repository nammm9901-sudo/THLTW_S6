using System.Collections.Generic;
using System.Linq;
using NoiThatCaoCap.Models;

namespace NoiThatCaoCap.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _products;

        public MockProductRepository()
        {
            _products = new List<Product>
            {
                new Product { Id = 1, Name = "Sofa Da Nhập Khẩu Ý", Price = 150000000, Description = "Sofa nhập khẩu Ý cao cấp", ImageUrl = "/images/sofa.jpg", Material = "Da thuộc", Origin = "Ý", CategoryId = 1 },
                new Product { Id = 2, Name = "Bàn Ăn Hoàng Gia Toát Đỉnh", Price = 220000000, Description = "Bàn ăn phong cách hoàng gia", ImageUrl = "/images/dining.jpg", Material = "Đá Marble", Origin = "Châu Âu", CategoryId = 2 },
                new Product { Id = 3, Name = "Giường Ngủ Vương Giả", Price = 180000000, Description = "Giường ngủ chuẩn khách sạn 5 sao", ImageUrl = "/images/bed.jpg", Material = "Gỗ Óc Chó", Origin = "Pháp", CategoryId = 3 },
                new Product { Id = 4, Name = "Đèn Thấu Kính Cung Điện", Price = 75000000, Description = "Đèn trang trí phong cách châu Âu", ImageUrl = "/images/lamp.jpg", Material = "Pha lê", Origin = "Áo", CategoryId = 4 },
                new Product { Id = 5, Name = "Tủ Trưng Bày Cổ Điển", Price = 95000000, Description = "Tủ trưng bày nội thất cao cấp", ImageUrl = "/images/cabinet.jpg", Material = "Gỗ Anh Đào", Origin = "Đức", CategoryId = 4 },
                new Product { Id = 6, Name = "Vật Phẩm Decor Nghệ Thuật", Price = 45000000, Description = "Decor nghệ thuật sang trọng", ImageUrl = "/images/decor.jpg", Material = "Thép nghệ thuật", Origin = "Việt Nam", CategoryId = 4 },
                new Product { Id = 7, Name = "Ghế Đơn Quý Tộc", Price = 68000000, Description = "Ghế đơn phong cách quý tộc", ImageUrl = "/images/chair.jpg", Material = "Nhung", Origin = "Ý", CategoryId = 1 },
                new Product { Id = 8, Name = "Bàn Trà Mạ Vàng Thượng Lưu", Price = 89000000, Description = "Bàn trà mạ vàng cao cấp", ImageUrl = "/images/teatable.jpg", Material = "Inox mạ PVD", Origin = "Việt Nam", CategoryId = 1 },
                new Product { Id = 9, Name = "Tủ Quần Áo Hoàng Gia", Price = 250000000, Description = "Tủ quần áo hoàng gia sang trọng", ImageUrl = "/images/wardrobe.jpg", Material = "Gỗ tự nhiên", Origin = "Pháp", CategoryId = 3 },
                new Product { Id = 10, Name = "Đèn Chùm Pha Lê Thượng Lưu", Price = 320000000, Description = "Đèn chùm pha lê đẳng cấp thượng lưu", ImageUrl = "/images/chandelier.jpg", Material = "Pha lê Swarovski", Origin = "Tiệp Khắc", CategoryId = 4 }
            };
        }

        public IEnumerable<Product> GetAll()
        {
            return _products;
        }

        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }

        public void Update(Product product)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Material = product.Material;
                existingProduct.Origin = product.Origin;
                existingProduct.CategoryId = product.CategoryId;
            }
        }

        public void Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }
    }
}