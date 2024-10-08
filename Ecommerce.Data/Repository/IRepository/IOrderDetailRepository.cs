using Ecommerce.Models;
using Ecommerce.Models.Models;


namespace Ecommerce.DataAcess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
       
    }
}
