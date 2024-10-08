using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
namespace Ecommerce.DataAcess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        internal DbSet<T>dbSet;
        public Repository(AppDbContext context)
        {
            _context=context;
            this.dbSet=_context.Set<T>();
            _context.Products.Include( u=>u.Category).Include(u=>u.CategoryId);
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
            
        }

        public T Get(Expression<Func<T, bool>> filter, string? includePropperties = null, bool tracked=false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else
            { 
                query=dbSet.AsNoTracking();
            }
            query= query.Where(filter);
            if (!string.IsNullOrEmpty(includePropperties))
            {
                foreach (var prop in includePropperties
               .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includePropperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includePropperties))
            {
                foreach (var prop in includePropperties
               .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    query = query.Include(prop);
                } 
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
          dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
