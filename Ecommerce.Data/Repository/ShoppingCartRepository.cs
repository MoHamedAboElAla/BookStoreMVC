using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.Models;
using System.Linq.Expressions;


namespace Ecommerce.DataAcess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {

        private readonly AppDbContext _context;
        public ShoppingCartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(ShoppingCart shoppingCart)
        {
            _context.Update(shoppingCart);
        }
    }
}
