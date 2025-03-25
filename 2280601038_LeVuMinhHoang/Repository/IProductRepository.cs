using _2280601038_LeVuMinhHoang.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _2280601038_LeVuMinhHoang.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}