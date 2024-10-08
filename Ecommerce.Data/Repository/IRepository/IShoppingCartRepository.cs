using Ecommerce.Models;
using Ecommerce.Models.Models;


namespace Ecommerce.DataAcess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
       
    }
}
