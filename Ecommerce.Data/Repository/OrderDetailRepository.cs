using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.Models;
using System.Linq.Expressions;


namespace Ecommerce.DataAcess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {

        private readonly AppDbContext _context;
        public OrderDetailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(OrderDetail orderDetail)
        {
            _context.OrderDetails.Update(orderDetail);
        }
    }
}
