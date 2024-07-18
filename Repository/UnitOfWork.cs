using dotSocialNetwork.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace dotSocialNetwork.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private dotSocialNetworkContext _db;
        private Dictionary<Type, object> _repositories;
        public UnitOfWork(dotSocialNetworkContext db)
        {
            this._db = db;
        }
        public void Dispose()
        {
        }
        public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = true) where TEntity : class
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<Type, object>();
            }
            if (hasCustomRepository)
            {
                var customRepo = _db.GetService<IRepository<TEntity>>();
                if (customRepo != null)
                {
                    return customRepo;
                }
            }
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<TEntity>(_db);
            }
            return (IRepository<TEntity>)_repositories[type];
        }
        public int SaveChanges(bool ensureAutoHistory = false)
        {
            throw new NotImplementedException();
        }
    }
}
