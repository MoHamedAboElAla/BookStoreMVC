using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.Models;
using System.Linq.Expressions;


namespace Ecommerce.DataAcess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {

        private readonly AppDbContext _context;
        public ApplicationUserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(ApplicationUser applicationUser)
        {
            _context.ApplicationUsers.Update(applicationUser);
        }
    }
}
