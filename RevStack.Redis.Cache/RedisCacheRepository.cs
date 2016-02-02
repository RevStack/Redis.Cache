using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using RevStack.Pattern;
using RevStack.Cache;
using System.Linq.Expressions;

namespace RevStack.Redis.Cache
{
    public class RedisCacheRepository<TEntity, TKey> : ICacheRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private const double DEFAULT_EXPIRATION = 2;
        private readonly IRedisClient _client;
        private readonly IRedisTypedClient<RedisCacheEntity<TEntity>> _typedClient;
        private string _domain;
        private CacheItemPolicy _policy;
        private double _hours=DEFAULT_EXPIRATION;

        public double Hours
        {
            get
            {
                return _hours;
            }

            set
            {
                _hours = value;
                _policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(_hours);
            }
        }


        #region "Public"
        public RedisCacheRepository(RedisDataContext context)
        {
            _client = context.Client();
            _typedClient = _client.As<RedisCacheEntity<TEntity>>();
            setDomain();
        }
      
        public TEntity Get(string key)
        {
            string id = _domain + key;
            var entity = _typedClient.GetById(id);
            if (DateTime.Now > _policy.AbsoluteExpiration)
            {
                _typedClient.Delete(entity);
                return null;
            }
            else
            {
                if(entity !=null && entity.Entity !=null) return entity.Entity;
                else return null;
            }
        }

        public IEnumerable<TEntity> Get(string key, bool isEnumerable)
        {
            string id = _domain + key;
            var entity = _typedClient.GetById(id);
            if (DateTime.Now > _policy.AbsoluteExpiration)
            {
                _typedClient.Delete(entity);
                return null;
            }
            else
            {
                if (entity != null && entity.Collection !=null) return entity.Collection;
                else return null;
            }
        }

        public void Set(string key, TEntity entity)
        {
            var cacheEntity = new RedisCacheEntity<TEntity>
            {
                Id=_domain + key,
                Domain=_domain,
                Entity=entity,
                AbsoluteExpiration= DateTime.Now.AddHours(_hours)
            };

            _typedClient.Store(cacheEntity);
        }

        public void Set(string key, IEnumerable<TEntity> entity, bool isEnumerable)
        {
            var cacheEntity = new RedisCacheEntity<TEntity>
            {
                Id = _domain + key,
                Domain = _domain,
                Collection = entity,
                AbsoluteExpiration = DateTime.Now.AddHours(_hours)
            };

            _typedClient.Store(cacheEntity);
        }

        public void Remove(string key)
        {
            string id = _domain + key;
            var entity = _typedClient.GetById(id);
            _typedClient.Delete(entity);
        }

        public void Clear()
        {
            var entries = _typedClient.GetAll().AsQueryable().Where(x => x.Domain == _domain);
            foreach(var entry in entries)
            {
                _typedClient.Delete(entry);
            }
        }

        public IEnumerable<TEntity> Get()
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public TEntity Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region "Private"
        private void setDomain()
        {
            _domain = AppDomain.CurrentDomain.FriendlyName;
            _policy = new CacheItemPolicy();
            _policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(_hours);
        }
        #endregion
    }
}
