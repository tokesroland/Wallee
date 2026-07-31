using Wallee.Models;

namespace Wallee.Services;

public interface ICategoryService
{
    Task<List<Category>> GetActiveAsync();
}