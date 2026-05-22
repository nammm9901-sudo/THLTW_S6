using System.Collections.Generic;
using System.Linq;
using NoiThatCaoCap.Models;

namespace NoiThatCaoCap.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;

        public MockCategoryRepository()
        {
            _categories = new List<Category>
            {
                // CategoryId = 1: Chứa Luxury Sofa (1), Royal Armchair (7), Golden Coffee Table (8)
                new Category
                {
                    Id = 1,
                    Name = "Sofa & Ghế Phòng Khách",
                    Description = "Các mẫu sofa nhập khẩu, ghế đơn quý tộc và bàn trà mạ vàng thượng lưu."
                },

                // CategoryId = 2: Chứa Royal Dining (2)
                new Category
                {
                    Id = 2,
                    Name = "Bàn Ghế Phòng Ăn",
                    Description = "Bộ bàn ăn bộ sưu tập hoàng gia, kiến tạo không gian ấm cúng và sang trọng."
                },

                // CategoryId = 3: Chứa Premium Bed (3), Imperial Wardrobe (9)
                new Category
                {
                    Id = 3,
                    Name = "Nội Thất Phòng Ngủ",
                    Description = "Giường ngủ chuẩn khách sạn 5 sao và tủ quần áo thiết kế biệt thự."
                },

                // CategoryId = 4: Chứa Luxury Lamp (4), Classic Cabinet (5), Premium Decor (6), Diamond Chandelier (10)
                new Category
                {
                    Id = 4,
                    Name = "Đèn & Tủ Trang Trí độc Bản",
                    Description = "Hệ thống đèn chùm pha lê cao cấp, tủ trưng bày và vật phẩm decor nghệ thuật."
                }
            };
        }

        public IEnumerable<Category> GetAll()
        {
            return _categories;
        }

        public Category GetById(int id)
        {
            return _categories.FirstOrDefault(c => c.Id == id);
        }
    }
}