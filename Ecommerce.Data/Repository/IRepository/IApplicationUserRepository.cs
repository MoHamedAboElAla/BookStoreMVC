using Ecommerce.Models;
using Ecommerce.Models.Models;


namespace Ecommerce.DataAcess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser applicationUser);
       
    }
}
