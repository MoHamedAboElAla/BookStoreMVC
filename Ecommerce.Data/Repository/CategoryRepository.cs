using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models;

using System.Linq.Expressions;


namespace Ecommerce.DataAcess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {

        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
