using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.Models;
using System.Linq.Expressions;


namespace Ecommerce.DataAcess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {

        private readonly AppDbContext _context;
        public OrderHeaderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb=_context.OrderHeaders.FirstOrDefault(i=>i.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus= orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }

            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _context.OrderHeaders.FirstOrDefault(i => i.Id == id);
                if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId= paymentIntentId;
                orderFromDb.PaymentDate= DateTime.Now;
            }
        }
    }
}
