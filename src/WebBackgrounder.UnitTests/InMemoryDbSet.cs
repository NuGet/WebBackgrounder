using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace WebBackgrounder.UnitTests
{
    public class InMemoryDbSet<TEntity> : IDbSet<TEntity> where TEntity : class
    {
        readonly HashSet<TEntity> _set;
        readonly IQueryable<TEntity> _queryableSet;

        public InMemoryDbSet()
            : this(Enumerable.Empty<TEntity>())
        {
        }

        public InMemoryDbSet(IEnumerable<TEntity> entities)
        {
            _set = new HashSet<TEntity>();
            foreach (var entity in entities)
            {
                _set.Add(entity);
            }
            _queryableSet = _set.AsQueryable();
        }

        public TEntity Add(TEntity entity)
        {
            _set.Add(entity);
            return entity;
        }

        public TEntity Attach(TEntity entity)
        {
            _set.Add(entity);
            return entity;
        }
        
        public TEntity Remove(TEntity entity)
        {
            _set.Remove(entity);
            return entity;
        }

        public virtual TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            throw new NotImplementedException();
        }

        public virtual TEntity Create()
        {
            throw new NotImplementedException();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            throw new NotImplementedException();    
        }

        public ObservableCollection<TEntity> Local
        {
            get { return new ObservableCollection<TEntity>(_queryableSet); }
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _queryableSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queryableSet.GetEnumerator();
        }

        public Type ElementType
        {
            get { return _queryableSet.ElementType; }
        }

        public Expression Expression
        {
            get { return _queryableSet.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _queryableSet.Provider; }
        }
    }
}
