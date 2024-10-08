using Ecommerce.Models;
using Ecommerce.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.DataAcess.Repository.IRepository
{
  public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}
