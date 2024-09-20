using Ecommerce.Models;


namespace Ecommerce.DataAcess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
       
    }
}
