using Ecommerce.DataAcess.Data;
using Ecommerce.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.DataAcess.Repository.IRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product Product)
        {
            var oldObjFromDb = _context.Products.FirstOrDefault(u=>u.Id==Product.Id);
            if (oldObjFromDb != null) { 
            oldObjFromDb.Title=Product.Title;
            oldObjFromDb.ISBN=Product.ISBN;
            oldObjFromDb.Price=Product.Price;
            oldObjFromDb.Description=Product.Description;
            oldObjFromDb.Price50=Product.Price50;
            oldObjFromDb.Price100=Product.Price100;
            oldObjFromDb.ListPrice=Product.ListPrice;
            oldObjFromDb.Author=Product.Author;
            oldObjFromDb.CategoryId=Product.CategoryId;
                if (Product.Image != null)
                {
                    oldObjFromDb.Image=Product.Image;
                }
            }
        }
    }
}
