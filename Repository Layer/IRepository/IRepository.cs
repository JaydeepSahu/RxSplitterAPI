using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository_Layer.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(int Id);
        T GetT(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetByExpression(Expression<Func<T, bool>> predicate);
        bool Insert(T entity);
        bool Update(T entity);
        bool Delete(T entity);
    }
}
