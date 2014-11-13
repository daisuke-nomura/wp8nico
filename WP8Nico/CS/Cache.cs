using System;
using System.Collections.Generic;

namespace WP8Nico.nomula
{
    public class Cache : IDisposable//シリアライズして自動的にローカルストレージに保存するように変える、通信だけじゃなく動画IDごとの情報も載せたい
    {
        private Dictionary<int, IEnumerable<RankingResults>> _cache;

        public Cache()
        {
            this._cache = new Dictionary<int, IEnumerable<RankingResults>>();
        }

        public void Clear()
        {
            if (this._cache != null)
                this._cache.Clear();
        }

        public void Reset()
        {
            if (this._cache != null)
                this._cache.Clear();
            else
                this._cache = new Dictionary<int, IEnumerable<RankingResults>>();
        }

        public bool ContainsKey(int key)
        {
            return this._cache != null ? this._cache.ContainsKey(key) : false;
        }

        public void Add(int key, IEnumerable<RankingResults> value)
        {
            this._cache.Add(key, value);
        }

        public bool TryGetValue(int key, out IEnumerable<RankingResults> value)
        {
            if (this._cache == null)
                this._cache = new Dictionary<int, IEnumerable<RankingResults>>();

            return this._cache.TryGetValue(key, out value);
        }

        public void Dispose()
        {
            this._cache = null;
        }
    }
}
