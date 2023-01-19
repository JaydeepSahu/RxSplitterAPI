//using DomainLayer.Data;
using DomainLayer.Data;
using DomainLayer.Common;
using Microsoft.EntityFrameworkCore;
using Repository_Layer.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections;

namespace Repository_Layer.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        #region property
        private readonly RxSplitterContext _context;
        private DbSet<T> entities;
        #endregion
        #region Constructor
        public Repository(RxSplitterContext context)
        {
            _context = context;
            entities = _context.Set<T>();
        }
        #endregion
        public virtual bool Delete(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                entities.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public T Get(int Id)
        {
            return entities.Find(Id);
        }

        public T GetT(Expression<Func<T, bool>> predicate)
        {
            return entities.Where(predicate).FirstOrDefault();
        }

        public async Task<List<T>> GetByExpression(Expression<Func<T, bool>> predicate)
        {
            return await entities.Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            return await entities.ToListAsync();
        }
        public bool Insert(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                  entities.Add(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public virtual bool Update(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                entities.Update(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
