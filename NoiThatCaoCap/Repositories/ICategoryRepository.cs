using System.Collections.Generic;
using NoiThatCaoCap.Models;

namespace NoiThatCaoCap.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category? GetById(int id); // Thêm dấu ? ở đây
    }
}