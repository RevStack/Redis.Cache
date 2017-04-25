using System;
using System.Collections.Generic;

namespace RevStack.Redis.Cache
{
    public class RedisCacheEntity<TEntity>
    {
        public string Id { get; set; }
        public string Domain { get; set; }
        public TEntity Entity { get; set; }
        public IEnumerable<TEntity> Collection { get; set; }
        public DateTime AbsoluteExpiration { get; set; }
        public DateTime LastCacheWriteTime { get; set; }
    }
}
