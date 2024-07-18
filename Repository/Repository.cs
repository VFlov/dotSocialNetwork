using dotSocialNetwork.Data;
using Microsoft.EntityFrameworkCore;

namespace dotSocialNetwork.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected dotSocialNetworkContext _db;
        public DbSet<T> Set { get; private set; }
        public Repository(dotSocialNetworkContext db)
        {
            _db = db;
            var set = _db.Set<T>();
            set.Load();
            Set= set;
        }
        public void Create(T item)
        {
            Set.Add(item);
            _db.SaveChanges();
        }
        public void Update(T item)
        {
            Set.Update(item);
            _db.SaveChanges();
        }
        public void Delete(T item)
        {
            Set.Remove(item);
            _db.SaveChanges();
        }
        public IEnumerable<T> GetAll()
        {
            return Set;
        }
        public T Get(int id)
        {
            return Set.Find(id);
        }

    }
}
