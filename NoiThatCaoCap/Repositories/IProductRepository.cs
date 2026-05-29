using System.Collections.Generic;
using System.Threading.Tasks;
using NoiThatCaoCap.Models;

namespace NoiThatCaoCap.Repositories 
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}